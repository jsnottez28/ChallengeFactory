namespace Domain.Entities;

// Reponse a la question "le projet professionnel explore jusqu'ici vous semble-t-il
// toujours pertinent ?" du questionnaire mi-parcours - nuance volontairement a 3 valeurs
// plutot qu'un simple oui/non binaire.
public enum PertinenceProjet
{
    Oui,
    Partiellement,
    Non,
}
