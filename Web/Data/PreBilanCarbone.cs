using Domain.Entities;

namespace Web.Data;

// Pre-diagnostic carbone : estimation rapide en approche monetaire (ratios ADEME Base
// Carbone), accessible sans compte, deposee par un prospect BtoB. Chaque depot est
// conserve avec les coordonnees du prospect (CRM) - jamais supprime, pour rester
// exploitable commercialement, au meme titre qu'une Reclamation (cf. Web/Data/Reclamation.cs
// pour le meme principe de tracabilite).
public class PreBilanCarbone
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Societe { get; set; }
    public string? Telephone { get; set; }

    // Informations de contexte optionnelles, saisies par le prospect - permettent de
    // qualifier le lead cote CRM sans etre bloquantes pour obtenir une premiere
    // estimation.
    public string? SecteurActivite { get; set; }
    public int? EffectifEtp { get; set; }
    public decimal? ChiffreAffairesKEuros { get; set; }

    public decimal TotalEmissionsKgCO2e { get; set; }

    public DateTime DeposeLe { get; set; } = DateTime.UtcNow;

    // Suivi commercial - jamais visible du prospect, cote Gestionnaire uniquement.
    public StatutPreBilanCrm StatutCrm { get; set; } = StatutPreBilanCrm.Nouveau;

    public List<PreBilanLigne> Lignes { get; set; } = [];
}
