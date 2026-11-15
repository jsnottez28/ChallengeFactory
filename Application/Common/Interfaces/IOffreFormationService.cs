using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class OffreFormationInput
{
    public string Titre { get; set; } = string.Empty;
    public CategorieOffreFormation Categorie { get; set; }
    public string? LienAvecOffre { get; set; }
    public string Duree { get; set; } = string.Empty;
    public string Public { get; set; } = string.Empty;
    public string? Financement { get; set; }
    public string Prix { get; set; } = "Sur devis";
    public bool DisponibleEnChallenge { get; set; } = true;
    public bool DisponibleEnPresentiel { get; set; }
    public ModePlateforme Cible { get; set; } = ModePlateforme.BtoB;
    public StatutOffreFormation Statut { get; set; } = StatutOffreFormation.SurDemande;
    public int? ChallengeId { get; set; }
    public string? LienPersonnalise { get; set; }
    public int Ordre { get; set; }
    public bool EstPublie { get; set; } = true;
}

// Back-office (/Administration/OffresFormation) + vitrine publique (/catalogue-formations).
// Cf. OffreFormation pour le mecanisme commun formations montees / formations sur demande.
public interface IOffreFormationService
{
    Task<List<OffreFormation>> GetAllAsync();

    Task<OffreFormation?> GetByIdAsync(int id);

    // Formations publiees, triees par Categorie puis Ordre puis Titre - utilise par la
    // vitrine publique.
    Task<List<OffreFormation>> GetPubliquesAsync();

    Task<(bool Success, string? ErrorMessage, OffreFormation? OffreFormation)> CreateAsync(OffreFormationInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, OffreFormationInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}
