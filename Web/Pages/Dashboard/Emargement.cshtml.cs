using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class EmargementModel(IEmargementService emargementService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int CohorteId { get; set; }

    [BindProperty]
    public List<int> EmargementIdsConfirmes { get; set; } = [];

    [BindProperty]
    public decimal? HeuresPresence { get; set; }

    [BindProperty]
    public decimal? HeuresTravailPersonnel { get; set; }

    // Rempli par le script du pad de signature (canvas.toDataURL) juste avant la
    // soumission - format "data:image/png;base64,...."
    [BindProperty]
    public string? SignatureDataUrl { get; set; }

    public EmargementPourSignatureInfo? Info { get; private set; }

    public bool RienASigner { get; private set; }

    public string? StatusMessage { get; private set; }

    public async Task OnGetAsync()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        Info = await emargementService.GetPourSignatureAsync(CohorteId, utilisateurId);
        RienASigner = Info is null || Info.Cartes.All(c => c.Signe);
        HeuresPresence = Info?.HeuresPresence;
        HeuresTravailPersonnel = Info?.HeuresTravailPersonnel;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        var (success, errorMessage) = await emargementService.SignerAsync(
            CohorteId, utilisateurId, EmargementIdsConfirmes, HeuresPresence, HeuresTravailPersonnel, DecoderSignature(SignatureDataUrl));

        StatusMessage = success ? "Votre émargement a été enregistré." : errorMessage;
        Info = await emargementService.GetPourSignatureAsync(CohorteId, utilisateurId);
        RienASigner = Info is null || Info.Cartes.All(c => c.Signe);

        return Page();
    }

    private static byte[]? DecoderSignature(string? dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl))
        {
            return null;
        }

        var virgule = dataUrl.IndexOf(',');
        if (virgule < 0)
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(dataUrl[(virgule + 1)..]);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
