using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class NotificationInfo
{
    public int Id { get; set; }
    public TypeNotification Type { get; set; }
    public string MessageCourt { get; set; } = string.Empty;
    public string Lien { get; set; } = string.Empty;
    public bool Lu { get; set; }
    public DateTime DateCreation { get; set; }
}

public interface INotificationService
{
    // Cree une notification pour un destinataire precis. Appele en effet de bord par les
    // actions declenchantes (jamais par une tache planifiee) - cf. ForumService et
    // PreuveService pour les points d'appel.
    Task CreerAsync(string utilisateurId, TypeNotification type, ReferenceTypeNotification referenceType, int referenceId, string messageCourt, string lien);

    // Les plus recentes en premier, limite raisonnable pour l'affichage en cloche.
    Task<List<NotificationInfo>> GetMesNotificationsAsync(string utilisateurId, int limite = 20);

    Task<int> GetNombreNonLuesAsync(string utilisateurId);

    Task<(bool Success, string? ErrorMessage, string? Lien)> MarquerLueAsync(int notificationId, string utilisateurId);

    Task MarquerToutesLuesAsync(string utilisateurId);
}
