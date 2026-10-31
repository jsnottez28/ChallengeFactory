namespace Domain.Entities;

// Seulement 2 etats persistes : "Programme" n'existe pas en base, c'est un libelle affiche
// (cf. ArticleService) derive de Publie + DatePublication dans le futur - la publication
// automatique decoule simplement d'une requete filtrant sur la date, jamais d'une tache
// planifiee (cf. CLAUDE.md - l'automatisation passe par n8n, pas par un scheduler .NET).
public enum StatutArticle
{
    Brouillon = 0,
    Publie = 1,
}
