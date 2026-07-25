using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class ChallengeInput
{
    public string Titre { get; set; } = string.Empty;
    public string? Slogan { get; set; }
    public int NombreEtapes { get; set; } = 8;
    public ModePlateforme Mode { get; set; }
}

public sealed class ChallengeEtapeInput
{
    public string TitreEtape { get; set; } = string.Empty;
    public string? ObjectifPedagogique { get; set; }
    public string? CompetenceCible { get; set; }
    public string? DefiIndividuel { get; set; }
}

public interface IChallengeService
{
    Task<List<Challenge>> GetAllAsync();

    Task<Challenge?> GetByIdAsync(int id);

    Task<ChallengeEtape?> GetEtapeByIdAsync(int etapeId);

    Task<(bool Success, string? ErrorMessage, Challenge? Challenge)> CreateAsync(ChallengeInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, ChallengeInput input);

    // Brouillon -> Publie. Un Challenge sans etape ne peut pas etre publie.
    Task<(bool Success, string? ErrorMessage)> PublierAsync(int id);

    // Le numero d'etape est assigne automatiquement (prochain numero libre) : le
    // "constructeur d'architecture etape par etape" ajoute toujours a la suite.
    Task<(bool Success, string? ErrorMessage, ChallengeEtape? Etape)> CreerEtapeAsync(int challengeId, ChallengeEtapeInput input);

    Task<(bool Success, string? ErrorMessage)> ModifierEtapeAsync(int etapeId, ChallengeEtapeInput input);

    Task<(bool Success, string? ErrorMessage)> SupprimerEtapeAsync(int etapeId);

    // Remplace integralement les Ressources Directrices de l'etape (selection multiple).
    Task<(bool Success, string? ErrorMessage)> DefinirCartesEtapeAsync(int etapeId, List<int> carteCompetenceIds);
}
