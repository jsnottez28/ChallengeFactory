using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test psychotechnique RIASEC - accessible a tout utilisateur connecte via un lien fixe
// (copie-colle manuellement dans le champ "Defi individuel" d'une etape, ou envoye
// manuellement), pas rattache a une Cohorte/etape particuliere - cf. IRiasecService.
[Authorize]
public class TestRiasecModel(IRiasecService riasecService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public List<int> Reponses { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public bool Repasser { get; set; }

    public List<RiasecQuestionInfo> Questions { get; private set; } = [];

    public RiasecResultatInfo? DernierResultat { get; private set; }

    public string? StatusMessage { get; private set; }

    public bool DernierEnvoiReussi { get; private set; }

    private bool echecSoumission;

    public bool AfficherQuestionnaire => DernierResultat is null || Repasser || echecSoumission;

    public async Task OnGetAsync()
    {
        Questions = riasecService.GetQuestions();
        var utilisateurId = userManager.GetUserId(User)!;
        DernierResultat = await riasecService.GetDernierResultatAsync(utilisateurId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Questions = riasecService.GetQuestions();
        var utilisateurId = userManager.GetUserId(User)!;

        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateurId, [.. Reponses]);

        DernierEnvoiReussi = success;
        echecSoumission = !success;
        StatusMessage = success ? "Merci, votre test est terminé. Votre profil est ci-dessous et vous a été envoyé par email." : errorMessage;
        DernierResultat = success ? resultat : await riasecService.GetDernierResultatAsync(utilisateurId);

        return Page();
    }
}
