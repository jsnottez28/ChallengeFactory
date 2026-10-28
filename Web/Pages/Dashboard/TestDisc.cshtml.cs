using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test psychotechnique DISC - accessible a tout le monde, y compris sans compte, via un lien
// fixe (copie-colle manuellement dans le champ "Defi individuel" d'une etape, ou envoye
// manuellement), pas rattache a une Cohorte/etape particuliere - cf. IDiscService. Un
// visiteur anonyme voit son resultat calcule a la volee (IDiscService.Calculer) mais jamais
// enregistre ni envoye par email ; un utilisateur connecte beneficie du parcours complet
// (persistance en historique + email), via IDiscService.RepondreAsync. Le CTA de fin de
// test pour un visiteur anonyme pointe vers /formations (catalogue BtoC), ou les actions
// "Rejoindre"/"Demander un embarquement" redirigent deja vers la creation de compte si
// necessaire - cf. HomeController.Formations/InscriptionFormation/DemanderEmbarquement.
public class TestDiscModel(IDiscService discService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public Dictionary<int, string> Reponses { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public bool Repasser { get; set; }

    public List<DiscQuestionInfo> Questions { get; private set; } = [];

    public DiscResultatInfo? DernierResultat { get; private set; }

    public string? StatusMessage { get; private set; }

    public bool DernierEnvoiReussi { get; private set; }

    public bool EstConnecte { get; private set; }

    private bool echecSoumission;

    // Affiche le questionnaire plutot que le dernier resultat quand aucun resultat n'existe
    // encore, quand l'utilisateur a explicitement demande a repasser le test, ou quand la
    // derniere soumission a echoue (pour corriger sans perdre le contexte).
    public bool AfficherQuestionnaire => DernierResultat is null || Repasser || echecSoumission;

    public async Task OnGetAsync()
    {
        Questions = discService.GetQuestions();
        EstConnecte = User.Identity?.IsAuthenticated == true;

        if (EstConnecte)
        {
            var utilisateurId = userManager.GetUserId(User)!;
            DernierResultat = await discService.GetDernierResultatAsync(utilisateurId);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Repasser est lue depuis la query string (SupportsGet=true) MEME sur ce POST : le
        // <form> n'a pas d'action explicite, donc il soumet vers l'URL courante - si
        // l'utilisateur est arrive ici via "?Repasser=true" (bouton "Repasser le test"),
        // cette valeur resterait vraie apres la soumission et masquerait le resultat
        // fraichement obtenu (AfficherQuestionnaire resterait vrai). On la neutralise ici :
        // une fois une soumission traitee, on affiche toujours soit le resultat frais, soit
        // (en cas d'echec) le formulaire via echecSoumission - jamais une intention
        // "repasser" perimee.
        Repasser = false;

        Questions = discService.GetQuestions();
        EstConnecte = User.Identity?.IsAuthenticated == true;

        if (EstConnecte)
        {
            var utilisateurId = userManager.GetUserId(User)!;
            var (success, errorMessage, resultat) = await discService.RepondreAsync(utilisateurId, Reponses);

            DernierEnvoiReussi = success;
            echecSoumission = !success;
            StatusMessage = success ? "Merci, votre test est terminé. Votre profil est ci-dessous et vous a été envoyé par email." : errorMessage;
            DernierResultat = success ? resultat : await discService.GetDernierResultatAsync(utilisateurId);
        }
        else
        {
            var (success, errorMessage, resultat) = discService.Calculer(Reponses);

            DernierEnvoiReussi = success;
            echecSoumission = !success;
            StatusMessage = success ? "Voici votre profil. Créez un compte pour le conserver dans votre historique." : errorMessage;
            DernierResultat = success ? resultat : null;
        }

        return Page();
    }
}
