using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers;

[Authorize]
[Route("Notifications")]
public class NotificationsController(INotificationService notificationService, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("{id:int}/Ouvrir")]
    public async Task<IActionResult> Ouvrir(int id)
    {
        var userId = userManager.GetUserId(User)!;
        var (success, _, lien) = await notificationService.MarquerLueAsync(id, userId);

        return Redirect(success && !string.IsNullOrWhiteSpace(lien) ? lien : "/Dashboard/Index");
    }

    [HttpPost("ToutesLues")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToutesLues(string? returnUrl)
    {
        var userId = userManager.GetUserId(User)!;
        await notificationService.MarquerToutesLuesAsync(userId);

        return LocalRedirect(!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/Dashboard/Index");
    }
}
