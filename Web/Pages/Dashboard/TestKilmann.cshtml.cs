using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test de gestion des conflits Thomas-Kilmann (TKI) - accessible a tout le monde, y compris
// sans compte, via un lien fixe (meme principe que TestDisc/TestRiasec/TestBigFive/TestSchein) :
// un visiteur anonyme voit son resultat calcule a la volee (IKilmannService.Calculer) mais
// jamais enregistre ni envoye par email ; un utilisateur connecte beneficie du parcours
// complet (persistance en historique + email), via IKilmannService.RepondreAsync. Le CTA de
// fin de test pour un visiteur anonyme pointe vers /formations (catalogue BtoC), ou les
// actions "Rejoindre"/"Demander un embarquement" redirigent deja vers la creation de compte
// si necessaire - cf. HomeController.Formations/InscriptionFormation/DemanderEmbarquement.
public class TestKilmannModel(IKilmannService kilmannService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public Dictionary<int, string> Reponses { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public bool Repasser { get; set; }

    public List<KilmannQuestionInfo> Questions { get; private set; } = [];

    public KilmannResultatInfo? DernierResultat { get; private set; }

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
        Questions = kilmannService.GetQuestions();
        EstConnecte = User.Identity?.IsAuthenticated == true;

        if (EstConnecte)
        {
            var utilisateurId = userManager.GetUserId(User)!;
            DernierResultat = await kilmannService.GetDernierResultatAsync(utilisateurId);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Repasser est lue depuis la query string (SupportsGet=true) MEME sur ce POST - meme
        // raison que TestDisc.cshtml.cs : on la neutralise ici, au point de finalisation.
        Repasser = false;

        Questions = kilmannService.GetQuestions();
        EstConnecte = User.Identity?.IsAuthenticated == true;

        if (EstConnecte)
        {
            var utilisateurId = userManager.GetUserId(User)!;
            var (success, errorMessage, resultat) = await kilmannService.RepondreAsync(utilisateurId, Reponses);

            DernierEnvoiReussi = success;
            echecSoumission = !success;
            StatusMessage = success ? "Merci, votre test est terminé. Votre profil est ci-dessous et vous a été envoyé par email." : errorMessage;
            DernierResultat = success ? resultat : await kilmannService.GetDernierResultatAsync(utilisateurId);
        }
        else
        {
            var (success, errorMessage, resultat) = kilmannService.Calculer(Reponses);

            DernierEnvoiReussi = success;
            echecSoumission = !success;
            StatusMessage = success ? "Voici votre profil. Créez un compte pour le conserver dans votre historique." : errorMessage;
            DernierResultat = success ? resultat : null;
        }

        return Page();
    }
}
