using Domain.Entities;

namespace Web.Data;

// Badge social (cf. CLAUDE.md, section Gaming) - distinct des badges hebdomadaires de
// competence (Badge/CarteCompetence). Calcule et attribue au moment de la cloture d'une
// etape (ICohorteService.ValiderEtapeAsync) : seul le resultat (qui a le badge) est
// expose, jamais le classement Points_Karma sous-jacent qui a servi au calcul (cf.
// prompt section 5, regle d'or).
public class BadgeSocialAttribution
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    public TypeBadgeSocial TypeBadge { get; set; }

    public DateTime DateAttribution { get; set; } = DateTime.UtcNow;
}
