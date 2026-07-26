using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class NotificationService(ApplicationDbContext dbContext) : INotificationService
{
    public async Task CreerAsync(string utilisateurId, TypeNotification type, ReferenceTypeNotification referenceType, int referenceId, string messageCourt, string lien)
    {
        dbContext.NotificationsInApp.Add(new NotificationInApp
        {
            UtilisateurId = utilisateurId,
            Type = type,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            MessageCourt = messageCourt,
            Lien = lien,
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task<List<NotificationInfo>> GetMesNotificationsAsync(string utilisateurId, int limite = 20)
    {
        return await dbContext.NotificationsInApp
            .Where(n => n.UtilisateurId == utilisateurId)
            .OrderByDescending(n => n.DateCreation)
            .Take(limite)
            .Select(n => new NotificationInfo
            {
                Id = n.Id,
                Type = n.Type,
                MessageCourt = n.MessageCourt,
                Lien = n.Lien,
                Lu = n.Lu,
                DateCreation = n.DateCreation,
            })
            .ToListAsync();
    }

    public async Task<int> GetNombreNonLuesAsync(string utilisateurId)
    {
        return await dbContext.NotificationsInApp.CountAsync(n => n.UtilisateurId == utilisateurId && !n.Lu);
    }

    public async Task<(bool Success, string? ErrorMessage, string? Lien)> MarquerLueAsync(int notificationId, string utilisateurId)
    {
        var notification = await dbContext.NotificationsInApp.FirstOrDefaultAsync(n => n.Id == notificationId);
        if (notification is null || notification.UtilisateurId != utilisateurId)
        {
            return (false, "Notification introuvable.", null);
        }

        notification.Lu = true;
        await dbContext.SaveChangesAsync();

        return (true, null, notification.Lien);
    }

    public async Task MarquerToutesLuesAsync(string utilisateurId)
    {
        var nonLues = await dbContext.NotificationsInApp
            .Where(n => n.UtilisateurId == utilisateurId && !n.Lu)
            .ToListAsync();

        foreach (var notification in nonLues)
        {
            notification.Lu = true;
        }

        await dbContext.SaveChangesAsync();
    }
}
