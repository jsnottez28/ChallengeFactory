namespace Domain.Entities;

// Distingue le FORMAT du parcours (independant de ThematiqueChallenge, qui porte sur le
// CONTENU) : Collectif (plusieurs apprenants dans une Cohorte, rythme et cartes partages -
// le fonctionnement historique de la plateforme) vs BilanCompetencesIndividuel (parcours
// solo, une Cohorte a un seul membre, methodologie bilan de competences). Collectif en
// premier (valeur 0) : c'est la valeur de repli correcte pour les Challenges DEJA CREES en
// base avant l'ajout de ce champ (backfill de la migration AddFormatChallenge) - tout le
// catalogue existant est un format collectif classique, jamais un bilan de competences
// individuel par defaut.
//
// Sert de garde-fou pour tout mecanisme reserve aux parcours individuels (ex. substitution
// des cartes proposees a un membre selon son diagnostic personnel, pour mieux coller a son
// bilan) : un tel mecanisme ne doit etre autorise QUE si Format ==
// BilanCompetencesIndividuel, jamais sur un Challenge Collectif ou la cohesion du groupe
// repose sur des cartes partagees par toute la Cohorte (cf. CLAUDE.md, principe Manifeste
// "l'equipe avant l'individu").
public enum FormatChallenge
{
    Collectif,
    BilanCompetencesIndividuel,
}
