using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test psychotechnique Big Five (modele OCEAN) - accessible a tout utilisateur connecte via
// un lien fixe (copie-colle manuellement dans le champ "Defi individuel" d'une etape, ou
// envoye manuellement), pas rattache a une Cohorte/etape particuliere - cf. IBigFiveService.
//
// Flux en 2 etapes : OnGetAsync affiche l'etape 1 (58 items, echelle de Likert).
// OnPostLikertAsync valide les 58 reponses et affiche l'etape 2 (5 paires de fiabilite a
// choix force), avec les reponses de l'etape 1 reportees en champs caches. OnPostFiabiliteAsync
// recoit les deux et finalise.
//
// Les reponses sont lues directement depuis Request.Form (prefixe "bf_"/"fiab_" + cle)
// plutot que via [BindProperty] sur un Dictionary<int,int> - meme principe que
// TestRiasec.cshtml.cs : le binder de dictionnaire d'ASP.NET Core s'est avere instable sur un
// flux a plusieurs formulaires avec report de valeurs en champs caches.
[Authorize]
public class TestBigFiveModel(IBigFiveService bigFiveService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Repasser { get; set; }

    // 1 = questionnaire Likert (58 items), 2 = round de fiabilite a choix force (5 paires).
    public int Etape { get; private set; } = 1;

    public List<BigFiveQuestionInfo> Questions { get; private set; } = [];
    public List<BigFiveFiabilitePaireInfo> PairesFiabilite { get; private set; } = [];
    public Dictionary<int, int> ReponsesPortees { get; private set; } = [];

    public BigFiveResultatInfo? DernierResultat { get; private set; }

    public string? StatusMessage { get; private set; }

    public bool DernierEnvoiReussi { get; private set; }

    private bool echecSoumission;

    // Affiche le questionnaire plutot que le dernier resultat quand aucun resultat n'existe
    // encore, quand l'utilisateur a explicitement demande a repasser le test, ou quand la
    // derniere soumission a echoue (pour corriger sans perdre le contexte).
    public bool AfficherQuestionnaire => DernierResultat is null || Repasser || echecSoumission;

    public async Task OnGetAsync()
    {
        Questions = bigFiveService.GetQuestions();
        var utilisateurId = userManager.GetUserId(User)!;
        DernierResultat = await bigFiveService.GetDernierResultatAsync(utilisateurId);
    }

    public IActionResult OnPostLikertAsync()
    {
        Questions = bigFiveService.GetQuestions();
        var reponses = LireReponses(Questions.Select(q => q.NumeroQuestion), "bf_");

        if (reponses.Count != Questions.Count)
        {
            StatusMessage = "Merci de répondre à toutes les affirmations avant de continuer.";
            DernierEnvoiReussi = false;
            echecSoumission = true;
            return Page();
        }

        Etape = 2;
        ReponsesPortees = reponses;
        PairesFiabilite = bigFiveService.GetPairesFiabilite();
        return Page();
    }

    public async Task<IActionResult> OnPostFiabiliteAsync()
    {
        // Repasser est lue depuis la query string (SupportsGet=true) MEME sur ce POST : les
        // <form> n'ont pas d'action explicite, donc ils soumettent vers l'URL courante - si
        // l'utilisateur est arrive ici via "?Repasser=true" (bouton "Repasser le test"),
        // cette valeur resterait vraie apres la soumission et masquerait le resultat
        // fraichement obtenu. On la neutralise ici, au point de finalisation : une fois le
        // test traite, on affiche toujours soit le resultat frais, soit (en cas d'echec)
        // l'etape 1 via echecSoumission - jamais une intention "repasser" perimee.
        Repasser = false;

        Questions = bigFiveService.GetQuestions();
        var reponses = LireReponses(Questions.Select(q => q.NumeroQuestion), "bf_");
        var pairesFiabilite = bigFiveService.GetPairesFiabilite();
        var reponsesFiabilite = LireReponses(pairesFiabilite.Select(p => p.PaireId), "fiab_");

        var utilisateurId = userManager.GetUserId(User)!;
        var (success, errorMessage, resultat) = await bigFiveService.RepondreAsync(utilisateurId, reponses, reponsesFiabilite);

        DernierEnvoiReussi = success;
        echecSoumission = !success;
        StatusMessage = success ? "Merci, votre test est terminé. Votre profil est ci-dessous et vous a été envoyé par email." : errorMessage;
        DernierResultat = success ? resultat : await bigFiveService.GetDernierResultatAsync(utilisateurId);

        if (!success)
        {
            Etape = 1;
        }

        return Page();
    }

    private Dictionary<int, int> LireReponses(IEnumerable<int> cles, string prefixe)
    {
        var reponses = new Dictionary<int, int>();
        foreach (var cle in cles)
        {
            var valeur = Request.Form[prefixe + cle];
            if (valeur.Count > 0 && int.TryParse(valeur[0], out var note))
            {
                reponses[cle] = note;
            }
        }
        return reponses;
    }
}
