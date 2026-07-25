namespace Domain.Entities;

// Ressources Directrices d'une etape : jointure many-to-many entre ChallengeEtape et
// CarteCompetence (moteur de Cartes de Competences deja developpe, reutilise tel quel).
public class ChallengeEtapeCarte
{
    public int Id { get; set; }

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    public int CarteCompetenceId { get; set; }
    public CarteCompetence CarteCompetence { get; set; } = null!;
}
