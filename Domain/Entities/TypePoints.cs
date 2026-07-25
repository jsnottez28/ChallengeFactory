namespace Domain.Entities;

// Implementation concrete des "Points Standard / Augmentes" du Manifeste (cf. CLAUDE.md,
// section Gaming) : XP_Savoir = performance individuelle sur une carte/ressource,
// Points_Karma = entraide (superieur a XP_Savoir par action pour inciter l'altruisme),
// Points_Assiduite = regularite d'equipe/individuelle, distincte de la performance.
public enum TypePoints
{
    XPSavoir,
    PointsKarma,
    PointsAssiduite,
}
