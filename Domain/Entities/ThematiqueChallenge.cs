namespace Domain.Entities;

// Distingue les deux grandes familles de Challenges du catalogue (prompt utilisateur :
// "je veux distinguer graphiquement les parcours sur la decarbonation des parcours sur
// l'humain dans l'entreprise"). HumainEtOrganisation en premier (valeur 0) : c'est la
// valeur de repli correcte pour les Challenges DEJA CREES en base avant l'ajout de ce
// champ (backfill de la migration AddThematiqueChallenge) - le catalogue historique de la
// plateforme (gestion du temps, management, stress...) est anterieur au virage
// decarbonation et releve entierement de cette famille ; "Cap Bas Carbone" et les futurs
// Challenges decarbonation sont eux crees APRES ce champ, avec Thematique choisie
// explicitement (jamais par defaut).
public enum ThematiqueChallenge
{
    HumainEtOrganisation,
    Decarbonation,
}
