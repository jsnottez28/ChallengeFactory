using Domain.Entities;

namespace Web.Data;

// Notification in-app (cf. prompt "Notifications in-app", section B) : incite au retour
// regulier sur la plateforme, en complement des emails deja en place. Cree en effet de
// bord par les actions declenchantes (ForumService/PreuveService), jamais par une tache
// planifiee (meme principe transverse que le reste de la plateforme).
public class NotificationInApp
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public TypeNotification Type { get; set; }

    public ReferenceTypeNotification ReferenceType { get; set; }
    public int ReferenceId { get; set; }

    public string MessageCourt { get; set; } = string.Empty;
    public string Lien { get; set; } = string.Empty;

    public bool Lu { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
