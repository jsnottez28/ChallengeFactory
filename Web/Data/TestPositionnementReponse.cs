using Domain.Entities;

namespace Web.Data;

// Auto-evaluation d'un membre sur une etape precise (son objectif pedagogique, plus parlant
// que le titre des cartes qui la composent), dans le cadre d'une campagne TestPositionnement
// (amont ou aval). Une ligne par (TestPositionnement, Utilisateur, ChallengeEtape) - la
// soumission est atomique (cf. TestPositionnementService.RepondreAsync, qui exige une reponse
// pour chaque etape du Challenge en un seul envoi) : l'existence d'au moins une ligne pour un
// (TestPositionnement, Utilisateur) donne signifie "a deja repondu", jamais de reponse
// partielle laissee en base.
public class TestPositionnementReponse
{
    public int Id { get; set; }

    public int TestPositionnementId { get; set; }
    public TestPositionnement TestPositionnement { get; set; } = null!;

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    // 0 (aucune connaissance) a 10 (maitrise complete) - auto-evaluation, jamais une
    // question a bonne/mauvaise reponse (cf. TypeTestPositionnement).
    public int NiveauAutoEvalue { get; set; }

    public DateTime RepondueLe { get; set; } = DateTime.UtcNow;
}
