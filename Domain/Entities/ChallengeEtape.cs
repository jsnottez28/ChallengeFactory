namespace Domain.Entities;

// Une ligne par etape d'un Challenge (1 a NombreEtapes). Pas de defi collectif dans cette
// version (voir CLAUDE.md / prompt Moteur de Challenges v2, section 9) - uniquement
// individuel via DefiIndividuel.
public class ChallengeEtape
{
    public int Id { get; set; }

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = null!;

    public int NumeroEtape { get; set; }
    public string TitreEtape { get; set; } = null!;
    public string? ObjectifPedagogique { get; set; }
    public string? CompetenceCible { get; set; }
    public string? DefiIndividuel { get; set; }

    public List<ChallengeEtapeCarte> Cartes { get; set; } = [];
}
