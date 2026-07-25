using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class ChallengeInput
{
    // Identifiant stable optionnel, cf. Challenge.Code : sert de cle d'upsert pour
    // l'import Excel. Facultatif pour une creation manuelle via l'UI.
    public string? Code { get; set; }
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

// Reutilise ImportErreurLigne (defini dans ICarteCompetenceService.cs, meme namespace) :
// meme forme Feuille/Ligne/Champ/Raison, pas de raison d'en dupliquer une variante.
public sealed class ImportChallengeRapport
{
    public int ChallengesCrees { get; set; }
    public int ChallengesMisAJour { get; set; }
    public int EtapesCreees { get; set; }
    public int EtapesMisesAJour { get; set; }
    public int CartesRattachees { get; set; }
    public int ChallengesPublies { get; set; }
    public List<ImportErreurLigne> Erreurs { get; set; } = [];
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

    // Uniquement si Brouillon et sans aucune Cohorte rattachee (une Cohorte, meme
    // EnPreparation, "reserve" deja son Challenge modele) - sinon erreur explicite.
    Task<(bool Success, string? ErrorMessage)> SupprimerAsync(int id);

    // Import/synchronisation depuis un fichier .xlsx (feuilles "challenge" et "etapes",
    // cf. ChallengeService.ImporterAsync pour le detail des colonnes). Upsert par
    // challenge_code / (challenge_code, numero_etape) : ne supprime jamais un Challenge ou
    // une etape absente du fichier. Publie automatiquement en fin d'import les Challenges
    // dont la colonne "statut" du fichier demande "Publie" (seulement si >= 1 etape).
    Task<ImportChallengeRapport> ImporterAsync(Stream fichierXlsx);
}
