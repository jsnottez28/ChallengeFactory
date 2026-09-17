using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test psychotechnique DISC - accessible a tout utilisateur connecte via un lien fixe
// (copie-colle manuellement dans le champ "Defi individuel" d'une etape, ou envoye
// manuellement), pas rattache a une Cohorte/etape particuliere - cf. IDiscService.
[Authorize]
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

    private bool echecSoumission;

    // Affiche le questionnaire plutot que le dernier resultat quand aucun resultat n'existe
    // encore, quand l'utilisateur a explicitement demande a repasser le test, ou quand la
    // derniere soumission a echoue (pour corriger sans perdre le contexte).
    public bool AfficherQuestionnaire => DernierResultat is null || Repasser || echecSoumission;

    public async Task OnGetAsync()
    {
        Questions = discService.GetQuestions();
        var utilisateurId = userManager.GetUserId(User)!;
        DernierResultat = await discService.GetDernierResultatAsync(utilisateurId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Questions = discService.GetQuestions();
        var utilisateurId = userManager.GetUserId(User)!;

        var (success, errorMessage, resultat) = await discService.RepondreAsync(utilisateurId, Reponses);

        DernierEnvoiReussi = success;
        echecSoumission = !success;
        StatusMessage = success ? "Merci, votre test est terminé. Votre profil est ci-dessous et vous a été envoyé par email." : errorMessage;
        DernierResultat = success ? resultat : await discService.GetDernierResultatAsync(utilisateurId);

        return Page();
    }
}
