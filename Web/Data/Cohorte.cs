using Domain.Entities;

namespace Web.Data;

// Instance d'un Challenge pour un groupe d'apprenants donne. Vit dans Web.Data (et pas
// Domain.Entities) car elle porte une reference optionnelle vers Organisation
// (cloisonnement BtoB), qui vit elle-meme dans Web.Data.Entities.
public class Cohorte
{
    public int Id { get; set; }

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = null!;

    public string Nom { get; set; } = null!;

    // Informative uniquement : n'est jamais utilisee pour calculer une progression
    // automatique (voir CLAUDE.md / prompt Moteur de Challenges v2, section 2 et 9).
    public DateTime? DateLancement { get; set; }

    // Demarre a 1 quand la Cohorte passe Active. N'avance que par validation manuelle
    // (ICohorteService.ValiderEtapeAsync) - jamais par calcul de date.
    public int EtapeCourante { get; set; }

    public StatutCohorte Statut { get; set; } = StatutCohorte.EnPreparation;

    // Rempli uniquement en mode BtoB (cloisonnement : les membres/l'architecture d'une
    // Cohorte d'une entreprise ne doivent jamais etre exposes a une autre entreprise).
    public int? OrganisationId { get; set; }
    public Entities.Organisation? Organisation { get; set; }

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;

    public List<CohorteMembre> Membres { get; set; } = [];
}
