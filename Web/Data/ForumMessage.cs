using Domain.Entities;

namespace Web.Data;

// Forum d'echange scope Cohorte + ChallengeEtape (pas de forum global). Les etapes
// precedentes deja cloturees restent consultables (historique, jamais purge).
public class ForumMessage
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    public string AuteurId { get; set; } = string.Empty;
    public ApplicationUser Auteur { get; set; } = null!;

    public string Contenu { get; set; } = string.Empty;

    // Reponse en fil - null pour un message racine.
    public int? MessageParentId { get; set; }
    public ForumMessage? MessageParent { get; set; }
    public List<ForumMessage> Reponses { get; set; } = [];

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public List<ForumMessageUtile> MarquagesUtile { get; set; } = [];
}
