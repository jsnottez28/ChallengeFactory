using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.ViewComponents;

public class NotificationBellViewComponent(INotificationService notificationService, UserManager<ApplicationUser> userManager) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = userManager.GetUserId(UserClaimsPrincipal);
        if (userId is null)
        {
            return View(new NotificationBellViewModel(0, []));
        }

        var nombreNonLues = await notificationService.GetNombreNonLuesAsync(userId);
        var notifications = await notificationService.GetMesNotificationsAsync(userId, limite: 10);

        return View(new NotificationBellViewModel(nombreNonLues, notifications));
    }
}

public record NotificationBellViewModel(int NombreNonLues, List<NotificationInfo> Notifications);
