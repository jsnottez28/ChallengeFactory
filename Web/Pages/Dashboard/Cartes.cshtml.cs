using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class CartesModel(ICarteApprenantService carteApprenantService, UserManager<ApplicationUser> userManager) : PageModel
{
    public List<CarteCompetence> MesCartes { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        MesCartes = await carteApprenantService.GetMesCartesAsync(userId);
        return Page();
    }
}
