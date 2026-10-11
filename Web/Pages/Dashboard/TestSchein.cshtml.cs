using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test des ancres de carriere (Schein, 1990 / Cerdin, 2007) - accessible a tout utilisateur
// connecte via un lien fixe (meme principe que TestRiasec/TestBigFive), pas rattache a une
// Cohorte/etape particuliere - cf. IScheinService.
//
// Flux en 2 etapes : OnGetAsync affiche l'etape 1 (45 affirmations, echelle de Likert).
// OnPostLikertAsync valide les 45 reponses et affiche l'etape 2 (choisir exactement 3
// affirmations parmi les 45, qui ajoutent chacune 4 points a leur ancre - cf. ScheinService),
// avec les reponses de l'etape 1 reportees en champs caches. OnPostChoixAsync recoit les deux
// et finalise.
//
// Les reponses sont lues directement depuis Request.Form (prefixe "sc_") plutot que via
// [BindProperty] sur un Dictionary<int,int> - meme principe que TestBigFive.cshtml.cs : le
// binder de dictionnaire d'ASP.NET Core s'est avere instable sur un flux a plusieurs
// formulaires avec report de valeurs en champs caches.
[Authorize]
public class TestScheinModel(IScheinService scheinService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Repasser { get; set; }

    // 1 = questionnaire Likert (45 items), 2 = choix des 3 affirmations prioritaires.
    public int Etape { get; private set; } = 1;

    public List<ScheinQuestionInfo> Questions { get; private set; } = [];
    public Dictionary<int, int> ReponsesPortees { get; private set; } = [];

    public ScheinResultatInfo? DernierResultat { get; private set; }

    public string? StatusMessage { get; private set; }

    public bool DernierEnvoiReussi { get; private set; }

    private bool echecSoumission;

    public bool AfficherQuestionnaire => DernierResultat is null || Repasser || echecSoumission;

    public async Task OnGetAsync()
    {
        Questions = scheinService.GetQuestions();
        var utilisateurId = userManager.GetUserId(User)!;
        DernierResultat = await scheinService.GetDernierResultatAsync(utilisateurId);
    }

    public IActionResult OnPostLikertAsync()
    {
        Questions = scheinService.GetQuestions();
        var reponses = LireReponsesLikert();

        if (reponses.Count != Questions.Count)
        {
            StatusMessage = "Merci de répondre à toutes les affirmations avant de continuer.";
            DernierEnvoiReussi = false;
            echecSoumission = true;
            return Page();
        }

        Etape = 2;
        ReponsesPortees = reponses;
        return Page();
    }

    public async Task<IActionResult> OnPostChoixAsync()
    {
        // Repasser est lue depuis la query string (SupportsGet=true) MEME sur ce POST : meme
        // raison que TestBigFive.cshtml.cs (les <form> soumettent vers l'URL courante, qui
        // peut porter "?Repasser=true" si l'utilisateur est arrive via le bouton "Repasser le
        // test") - on la neutralise ici, au point de finalisation.
        Repasser = false;

        Questions = scheinService.GetQuestions();
        var reponses = LireReponsesLikert();
        var choixPrioritaires = Request.Form["choix"]
            .Select(valeur => int.TryParse(valeur, out var numero) ? numero : (int?)null)
            .Where(numero => numero.HasValue)
            .Select(numero => numero!.Value)
            .ToList();

        var utilisateurId = userManager.GetUserId(User)!;
        var (success, errorMessage, resultat) = await scheinService.RepondreAsync(utilisateurId, reponses, choixPrioritaires);

        DernierEnvoiReussi = success;
        echecSoumission = !success;
        StatusMessage = success ? "Merci, votre test est terminé. Votre profil est ci-dessous et vous a été envoyé par email." : errorMessage;
        DernierResultat = success ? resultat : await scheinService.GetDernierResultatAsync(utilisateurId);

        if (!success)
        {
            // Si les 45 reponses Likert etaient valides mais pas le choix des 3 affirmations,
            // on repart directement a l'etape 2 (pas la peine de refaire les 45 affirmations)
            // avec les memes reponses reportees en champs caches.
            Etape = reponses.Count == Questions.Count ? 2 : 1;
            ReponsesPortees = reponses;
        }

        return Page();
    }

    private Dictionary<int, int> LireReponsesLikert()
    {
        var reponses = new Dictionary<int, int>();
        foreach (var question in scheinService.GetQuestions())
        {
            var valeur = Request.Form["sc_" + question.NumeroQuestion];
            if (valeur.Count > 0 && int.TryParse(valeur[0], out var note))
            {
                reponses[question.NumeroQuestion] = note;
            }
        }
        return reponses;
    }
}
