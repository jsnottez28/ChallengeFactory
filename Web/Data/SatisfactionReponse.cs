namespace Web.Data;

// Reponse a l'enquete de satisfaction envoyee automatiquement a la cloture d'une Cohorte
// (cf. CohorteService.NotifierClotureAsync) - alimente le taux de satisfaction/NPS du
// critere 7 du Referentiel National Qualite (Qualiopi). Score type NPS (0 a 10), aligne
// sur le standard le plus largement reconnu pour ce type d'enquete - a ne pas confondre
// avec une note sur 20 (utilisee ailleurs de facon purement informative sur des supports
// marketing, jamais dans ce modele).
public class SatisfactionReponse
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    // 0 (pas du tout probable) a 10 (tres probable) - question NPS standard
    // ("Recommanderiez-vous ce Challenge a un collegue ?").
    public int Score { get; set; }

    public string? Commentaire { get; set; }

    public DateTime RepondueLe { get; set; } = DateTime.UtcNow;
}
