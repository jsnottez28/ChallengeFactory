using Domain.Entities;

namespace Web.Data;

// Le livrable de validation de competence (cf. CLAUDE.md, "La preuve remplace le QCM").
// Une seule Preuve par (Utilisateur, ChallengeEtape) : elle est modifiee/completee en
// place tant qu'elle n'est pas ValideeDefinitivement, jamais recreee (voir
// IPreuveService, unicite imposee par index en base).
public class Preuve
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime DateDepot { get; set; } = DateTime.UtcNow;

    public StatutPreuve Statut { get; set; } = StatutPreuve.Soumise;

    public List<PreuveFichier> Fichiers { get; set; } = [];
    public List<PreuveValidationPair> ValidationsPairs { get; set; } = [];
    public List<PreuveValidationGestionnaire> ValidationsGestionnaire { get; set; } = [];
}
