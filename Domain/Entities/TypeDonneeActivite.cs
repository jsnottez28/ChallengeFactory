namespace Domain.Entities;

// Distingue les deux familles de facteurs d'emission du Pre-diagnostic carbone (cf.
// FacteurEmission) :
//   - Monetaire : l'utilisateur saisit un montant en euros, le facteur est deja normalise
//     en kgCO2e/euro (les ratios sources en kgCO2e/k-euro sont divises par 1000 a la
//     saisie) ;
//   - Physique : l'utilisateur saisit une quantite physique (kWh, km...), l'unite exacte
//     est portee par FacteurEmission.Unite.
public enum TypeDonneeActivite
{
    Monetaire,
    Physique,
}
