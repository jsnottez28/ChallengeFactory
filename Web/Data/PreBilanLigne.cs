namespace Web.Data;

// Une ligne de saisie du Pre-diagnostic : un FacteurEmission, la donnee d'activite saisie
// par le prospect (montant en euros ou quantite physique selon
// FacteurEmission.TypeDonnee), et l'emission calculee cote serveur - jamais confiee au
// client. Meme forme que les lignes de calcul du fichier Bilan Carbone(R) source
// ("Libelle personnalise | Donnee d'activite | Facteur d'emission | Emissions").
public class PreBilanLigne
{
    public int Id { get; set; }

    public int PreBilanCarboneId { get; set; }
    public PreBilanCarbone PreBilanCarbone { get; set; } = null!;

    public int FacteurEmissionId { get; set; }
    public FacteurEmission FacteurEmission { get; set; } = null!;

    public decimal ValeurSaisie { get; set; }

    public decimal EmissionsKgCO2e { get; set; }
}
