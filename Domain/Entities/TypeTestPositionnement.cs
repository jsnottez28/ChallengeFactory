namespace Domain.Entities;

// Test de connaissances (Methode Miroir, cf. CLAUDE.md "Mesure d'impact & KPI") : mesure
// de progression par auto-evaluation avant/apres le parcours, jamais une validation de
// competence (pas de bonne/mauvaise reponse, pas de QCM note) - cf. le point de tension
// Manifeste/Procedure sur le QCM, deja tranche pour ce cas precis.
public enum TypeTestPositionnement
{
    Amont,
    Aval,
}
