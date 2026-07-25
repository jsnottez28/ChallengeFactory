namespace Domain.Entities;

// Le "modele" de parcours (cf. CLAUDE.md, cycle de vie d'un Challenge). Une Cohorte est
// une instance de ce modele. Un Challenge en Brouillon ne peut pas servir a creer une
// Cohorte (voir ICohorteService.CreerAsync).
public class Challenge
{
    public int Id { get; set; }

    // Identifiant stable optionnel (ex. "CHAL-ENGAGER-EQUIPE-PROJET") : sert de cle
    // d'upsert pour l'import Excel (cf. IChallengeService.ImporterAsync), sur le meme
    // principe que CarteCompetence.Code. Facultatif pour un Challenge cree manuellement
    // via l'UI - d'ou l'index unique filtre (plusieurs NULL autorises).
    public string? Code { get; set; }

    public string Titre { get; set; } = null!;
    public string? Slogan { get; set; }

    public int NombreEtapes { get; set; } = 8;

    public ModePlateforme Mode { get; set; }
    public StatutChallenge Statut { get; set; } = StatutChallenge.Brouillon;

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;

    public List<ChallengeEtape> Etapes { get; set; } = [];
}
