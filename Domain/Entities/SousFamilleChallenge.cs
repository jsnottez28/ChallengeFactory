namespace Domain.Entities;

// Classement fin de la vitrine /formations, a cote de ThematiqueChallenge (qui ne porte
// que 2 valeurs et reste utilise pour l'accent visuel Decarbonation/Humain&Organisation).
// Reprend les 8 familles du document "Catalogue de formations Challenges Factory" -
// nullable sur Challenge : les Challenges deja crees avant ce champ n'en ont pas besoin
// pour continuer a s'afficher.
public enum SousFamilleChallenge
{
    Sensibiliser,
    DuBilanALAction,
    PiloterTrajectoireSnbc,
    ManagerEtEmbarquer,
    DefisParPosteEmission,
    IaEtTransition,
    InspireesDuGraine,
    ParticuliersEtReconversion,
}
