using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test de connaissances amont/aval (Methode Miroir) : le membre auto-evalue son niveau
// (0-10) sur chaque carte du Challenge, en un seul envoi - cf. ITestPositionnementService.
[Authorize]
public class TestPositionnementModel(ITestPositionnementService testPositionnementService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int CohorteId { get; set; }

    [BindProperty(SupportsGet = true)]
    public TypeTestPositionnement Type { get; set; }

    [BindProperty]
    public Dictionary<int, int> Niveaux { get; set; } = [];

    public TestPositionnementInfo? Info { get; private set; }

    public bool DejaRepondu { get; private set; }

    public string? StatusMessage { get; private set; }

    public bool DernierEnvoiReussi { get; private set; }

    public async Task OnGetAsync()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        Info = await testPositionnementService.GetPourReponseAsync(CohorteId, Type, utilisateurId);
        DejaRepondu = await testPositionnementService.ADejaReponduAsync(CohorteId, Type, utilisateurId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var utilisateurId = userManager.GetUserId(User)!;

        var (success, errorMessage) = await testPositionnementService.RepondreAsync(CohorteId, Type, utilisateurId, Niveaux);

        DernierEnvoiReussi = success;
        StatusMessage = success ? "Merci, votre test a bien été enregistré." : errorMessage;
        Info = await testPositionnementService.GetPourReponseAsync(CohorteId, Type, utilisateurId);
        DejaRepondu = await testPositionnementService.ADejaReponduAsync(CohorteId, Type, utilisateurId);

        return Page();
    }
}
