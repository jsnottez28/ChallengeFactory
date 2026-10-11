using Domain.Entities;

namespace Application.Common.Interfaces;

// Facteur d'emission propose au prospect - cf. Web.Data.FacteurEmission pour le detail des
// champs.
public sealed class FacteurEmissionInfo
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public PosteEmission Poste { get; set; }
    public TypeDonneeActivite TypeDonnee { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Unite { get; set; } = string.Empty;
}

// Une ligne saisie par le prospect : cle = FacteurEmissionInfo.Code (jamais l'Id, plus
// stable cote formulaire), valeur = montant en euros ou quantite physique selon
// FacteurEmissionInfo.TypeDonnee. Une valeur nulle/zero est traitee comme "non renseigne".
public sealed class PreBilanCarboneInput
{
    public string Email { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Societe { get; set; }
    public string? Telephone { get; set; }
    public string? SecteurActivite { get; set; }
    public int? EffectifEtp { get; set; }
    public decimal? ChiffreAffairesKEuros { get; set; }
    public Dictionary<string, decimal> ValeursParCodeFacteur { get; set; } = [];
}

public sealed class PreBilanLigneResultatInfo
{
    public string Code { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public PosteEmission Poste { get; set; }
    public decimal ValeurSaisie { get; set; }
    public decimal EmissionsKgCO2e { get; set; }
}

public sealed class PreBilanPosteResultatInfo
{
    public PosteEmission Poste { get; set; }
    public decimal EmissionsTotalesKgCO2e { get; set; }
}

public sealed class PreBilanResultatInfo
{
    public int Id { get; set; }
    public decimal TotalEmissionsKgCO2e { get; set; }
    public List<PreBilanPosteResultatInfo> ParPoste { get; set; } = [];
    public List<PreBilanLigneResultatInfo> Lignes { get; set; } = [];
    public DateTime DeposeLe { get; set; }
}

// Vue CRM (Gestionnaire) d'un Pre-diagnostic depose.
public sealed class PreBilanCarboneInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Societe { get; set; }
    public string? Telephone { get; set; }
    public string? SecteurActivite { get; set; }
    public int? EffectifEtp { get; set; }
    public decimal? ChiffreAffairesKEuros { get; set; }
    public decimal TotalEmissionsKgCO2e { get; set; }
    public DateTime DeposeLe { get; set; }
    public StatutPreBilanCrm StatutCrm { get; set; }
}

// Pre-diagnostic carbone : estimation rapide en approche monetaire (ratios ADEME Base
// Carbone, extraits du fichier Bilan Carbone(R) V9.0 de l'Association Bilan Carbone -
// jamais nomme "Bilan Carbone(R)" cote public, marque deposee reservee a la methode
// complete realisee par un praticien). Concu pour pouvoir etre etendu vers un vrai Bilan
// Carbone multi-postes plus tard (cf. Domain.Entities.PosteEmission) - le score est
// toujours recalcule cote serveur a partir des facteurs stockes en base, jamais confie au
// client.
public interface IPreBilanCarboneService
{
    // Facteurs actifs, groupes par Poste - jamais de dimension exposee cote client au-dela
    // du Poste lui-meme (contrairement aux tests psychotechniques, ici la transparence sur
    // le facteur utilise fait partie de la demarche).
    Task<List<FacteurEmissionInfo>> GetFacteursDisponiblesAsync();

    // Calcule, enregistre (CRM) et envoie le resultat par email au prospect. Recalcule
    // integralement les emissions a partir des facteurs stockes en base - les valeurs
    // saisies sont les seules donnees de confiance venant du client.
    Task<(bool Success, string? ErrorMessage, PreBilanResultatInfo? Resultat)> DeposerAsync(PreBilanCarboneInput input);

    // Cote Gestionnaire (CRM) : liste de tous les Pre-diagnostics deposes.
    Task<List<PreBilanCarboneInfo>> GetTousAsync();

    Task<PreBilanResultatInfo?> GetResultatAsync(int id);

    Task<(bool Success, string? ErrorMessage)> ChangerStatutCrmAsync(int id, StatutPreBilanCrm statut);
}
