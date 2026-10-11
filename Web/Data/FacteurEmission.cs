using Domain.Entities;

namespace Web.Data;

// Facteur d'emission utilise par le Pre-diagnostic carbone - stocke en base (pas en dur
// dans le code) pour pouvoir etre etendu ou corrige sans deploiement, et pour servir de
// socle a un futur Bilan Carbone complet sur la plateforme (memes postes, meme structure
// "facteur + donnee d'activite", cf. PreBilanLigne). Chaque valeur est tracee jusqu'a la
// cellule source du fichier Bilan Carbone(R) V9.0 de l'Association Bilan Carbone (ratios
// ADEME Base Carbone, donnee publique - cf. commentaire de PreBilanCarboneService.Seed).
public class FacteurEmission
{
    public int Id { get; set; }

    // Cle stable, utilisee par le code (jamais l'Id auto-incremente) pour referencer un
    // facteur precis - ex. "ACHAT_IT". Ne change jamais une fois publiee : un
    // PreBilanLigne existant doit continuer a pointer vers le meme facteur.
    public string Code { get; set; } = string.Empty;

    public PosteEmission Poste { get; set; }

    public TypeDonneeActivite TypeDonnee { get; set; }

    // Libelle affiche a l'utilisateur du Pre-diagnostic (ex. "Informatique, logiciels et
    // hebergement").
    public string Nom { get; set; } = string.Empty;

    // "€" pour un facteur Monetaire, sinon l'unite physique exacte (ex. "kWh",
    // "véhicule.km", "passager.km", "passager eq.km").
    public string Unite { get; set; } = string.Empty;

    // Toujours exprime en kgCO2e par Unite (les facteurs monetaires sources en
    // kgCO2e/k-euro sont deja divises par 1000 a la saisie, cf. PreBilanCarboneService).
    public decimal ValeurKgCO2eParUnite { get; set; }

    // Tracabilite : nom exact de la ligne source dans le fichier Bilan Carbone(R) V9.0
    // (onglet FE Intrants / FE Immobilisations / FE Energie / FE Déplacements), jamais
    // reformule - permet de retrouver/mettre a jour la valeur si l'ABC publie une
    // nouvelle version.
    public string Source { get; set; } = string.Empty;

    public int Ordre { get; set; }

    public bool Actif { get; set; } = true;
}
