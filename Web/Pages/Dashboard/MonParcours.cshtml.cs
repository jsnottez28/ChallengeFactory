using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class MonParcoursModel(ICohorteService cohorteService, UserManager<ApplicationUser> userManager) : PageModel
{
    public List<ParcoursEnCoursInfo> Parcours { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        Parcours = await cohorteService.GetMesParcoursEnCoursAsync(userId);
        return Page();
    }
}
