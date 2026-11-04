namespace Domain.Entities;

// Cycle de vie du questionnaire de collecte d'un dossier client carbone :
// - Envoye : le Gestionnaire a envoye le lien, le client n'a encore rien enregistre ;
// - EnCours : le client a enregistre un brouillon (il peut revenir le completer) ;
// - Soumis : le client a envoye ses reponses - elles ne sont plus modifiables par lui
//   (le Gestionnaire peut le rouvrir en renvoyant le questionnaire).
public enum StatutQuestionnaireCollecte
{
    Envoye,
    EnCours,
    Soumis,
}
