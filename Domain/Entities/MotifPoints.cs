namespace Domain.Entities;

public enum MotifPoints
{
    // XP_Savoir, auteur de la preuve - un seul evenement par Preuve quel que soit le
    // chemin (validation directe Gestionnaire ou cloture d'etape en bloc).
    PreuveValideeDefinitivement,

    // Points_Karma, le pair validateur - genere par une decision Valide OU A revoir
    // (c'est l'effort de review qui est valorise, pas seulement l'approbation).
    DecisionPairDonnee,

    // Points_Karma, l'auteur du message de forum marque "utile".
    MessageForumUtile,

    // Points_Assiduite, auteur de la preuve - uniquement si la preuve etait au statut
    // ValideeParLesPairs au moment precis de la cloture d'etape (recompense la
    // reactivite a obtenir le consensus des pairs avant la cloture, distinct de
    // PreuveValideeDefinitivement qui peut etre gagne sans jamais passer par ce statut,
    // ex. validation directe Gestionnaire sur une preuve encore Soumise).
    PreuveValideeParLesPairsALaCloture,
}
