using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class CarteDetailModel(ICarteApprenantService carteApprenantService, UserManager<ApplicationUser> userManager) : PageModel
{
    public CarteCompetence? Carte { get; private set; }

    // Renvoie NotFound (jamais la carte) si elle n'est pas attribuee a l'utilisateur
    // courant : c'est le point d'application du controle d'acces serveur, empeche tout
    // acces par Id devine/force dans l'URL.
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        Carte = await carteApprenantService.GetCarteAttribueeAsync(userId, id);
        if (Carte is null)
        {
            return NotFound();
        }

        return Page();
    }
}
