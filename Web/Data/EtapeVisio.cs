namespace Web.Data;

// Rituel Synchrone (visio) propose a chaque etape d'un parcours (cf. CLAUDE.md, "Boucle
// CBL hebdomadaire" - Temps 3, Action), aussi bien pour un parcours standard que pour un
// bilan de competences (chaque seance y est, dans ce cas, une etape). Une ligne par
// (Cohorte, NumeroEtape) : le Gestionnaire saisit un lien externe (Zoom/Teams/Meet...) et
// une date, puis peut (re)envoyer ce lien a tous les membres - la plateforme n'heberge
// jamais elle-meme de visio.
public class EtapeVisio
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public int NumeroEtape { get; set; }

    public DateTime? DateVisio { get; set; }
    public string? LienVisio { get; set; }

    public string PlanifieParId { get; set; } = string.Empty;
    public ApplicationUser PlanifiePar { get; set; } = null!;

    public DateTime PlanifieLe { get; set; } = DateTime.UtcNow;
    public DateTime? DernierEnvoiLe { get; set; }
}
