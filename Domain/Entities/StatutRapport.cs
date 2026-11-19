namespace Domain.Entities;

// Seulement 2 etats persistes : "Programme" n'existe pas en base, c'est un libelle affiche
// (cf. RapportService) derive de Publie + DatePublication dans le futur - meme principe
// que StatutArticle.
public enum StatutRapport
{
    Brouillon = 0,
    Publie = 1,
}
