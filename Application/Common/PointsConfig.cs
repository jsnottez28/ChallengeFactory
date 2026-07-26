namespace Application.Common;

// Montants par defaut des points (cf. CLAUDE.md, section Gaming, et prompt "Depot de
// preuves, points et forum par etape", section 4) - centralises ici plutot que codes en
// dur disperses dans les services, pour rester facilement ajustables.
public static class PointsConfig
{
    // XP_Savoir : preuve passee ValideeDefinitivement (validation directe Gestionnaire
    // ou cloture d'etape), attribue a l'auteur.
    public const int XPSavoirPreuveValidee = 50;

    // Points_Karma : un pair donne une decision (Valide OU ARevoir) sur une preuve -
    // volontairement superieur a XP_Savoir pour inciter l'entraide plus que la
    // performance solo (cf. CLAUDE.md).
    public const int PointsKarmaDecisionPair = 75;

    // Points_Karma : un message de forum est marque "utile" par un pair.
    public const int PointsKarmaMessageUtile = 25;

    // Points_Assiduite : preuve au statut ValideeParLesPairs au moment precis de la
    // cloture d'etape (reactivite a obtenir le consensus des pairs avant la cloture).
    public const int PointsAssiduitePreuveValideeALaCloture = 20;

    // Seuil de validation par les pairs (section 3) : ratio Valide / total decisions.
    // Pas configurable par Challenge dans cette version (cf. prompt, section 8).
    public const double SeuilRatioValidationPairs = 0.5;
}
