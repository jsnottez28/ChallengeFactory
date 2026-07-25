namespace Domain.Entities;

// Le "modele" de parcours (cf. CLAUDE.md, cycle de vie d'un Challenge). Une Cohorte est
// une instance de ce modele. Un Challenge en Brouillon ne peut pas servir a creer une
// Cohorte (voir ICohorteService.CreerAsync).
public class Challenge
{
    public int Id { get; set; }

    public string Titre { get; set; } = null!;
    public string? Slogan { get; set; }

    public int NombreEtapes { get; set; } = 8;

    public ModeChallenge Mode { get; set; }
    public StatutChallenge Statut { get; set; } = StatutChallenge.Brouillon;

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;

    public List<ChallengeEtape> Etapes { get; set; } = [];
}
