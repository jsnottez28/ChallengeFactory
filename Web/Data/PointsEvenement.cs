using Domain.Entities;

namespace Web.Data;

// Journal des points - source de verite unique. Les totaux affiches (apprenant comme
// gestionnaire) sont TOUJOURS calcules par agregation de ce journal, jamais stockes
// comme un compteur mutable a part - necessaire pour l'auditabilite et pour eviter les
// doubles comptages (cf. prompt section 1). Voir PointsConfig pour les montants.
public class PointsEvenement
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int? CohorteId { get; set; }
    public Cohorte? Cohorte { get; set; }

    public TypePoints TypePoints { get; set; }
    public int Montant { get; set; }
    public MotifPoints Motif { get; set; }

    public ReferenceTypePoints ReferenceType { get; set; }
    public int ReferenceId { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
}
