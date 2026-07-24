using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class CarteCompetenceInput
{
    // ---- Face Theorie ----
    public string Code { get; set; } = string.Empty;
    public int? BadgeId { get; set; }
    public NiveauCarte Niveau { get; set; }
    public string TitreTheorie { get; set; } = string.Empty;

    public string? Objectif1 { get; set; }
    public string? Objectif2 { get; set; }
    public string? Objectif3 { get; set; }
    public string? Objectif4 { get; set; }

    public string? Citation { get; set; }
    public string? AuteurCitation { get; set; }

    // Nom de fichier deja enregistre sur disque (l'upload lui-meme est gere par le
    // controleur, pas par le service - voir CartesController).
    public string? ImageCarteA { get; set; }

    // ---- Face Defi ----
    public string? TitreDefi { get; set; }
    public string? ContextePro { get; set; }
    public string? ContextePerso { get; set; }
    public string? TonDefi { get; set; }

    public string? Etape1 { get; set; }
    public string? Etape2 { get; set; }
    public string? Etape3 { get; set; }
    public string? Etape4 { get; set; }
    public string? Etape5 { get; set; }

    public string? Tip1 { get; set; }
    public string? Tip2 { get; set; }
    public string? Tip3 { get; set; }
    public string? Tip4 { get; set; }
    public string? Tip5 { get; set; }

    public string? CitationHumour { get; set; }
    public string? LienVideo { get; set; }
}

public sealed class CarteCompetenceFiltre
{
    // Recherche libre : matche Code, TitreTheorie ou TitreDefi.
    public string? Recherche { get; set; }
    public NiveauCarte? Niveau { get; set; }
    public int? BadgeId { get; set; }
    public string? Programme { get; set; }
    public int Page { get; set; } = 1;
    public int TaillePage { get; set; } = 20;
}

public sealed class CarteCompetenceResultatRecherche
{
    public List<CarteCompetence> Cartes { get; set; } = [];
    public int NombreTotal { get; set; }
}

public sealed class ImportErreurLigne
{
    public string Feuille { get; set; } = string.Empty;
    public int Ligne { get; set; }
    public string? Champ { get; set; }
    public string Raison { get; set; } = string.Empty;
}

public sealed class ImportRapport
{
    public int BadgesCrees { get; set; }
    public int BadgesMisAJour { get; set; }
    public int CartesCreees { get; set; }
    public int CartesMisesAJour { get; set; }
    public List<ImportErreurLigne> Erreurs { get; set; } = [];
}

// DTO plutot que l'entite CarteAttribution : celle-ci vit dans Web.Data (references
// directes vers ApplicationUser) et Application n'a pas de reference au projet Web.
public sealed class CarteAttributionInfo
{
    public int Id { get; set; }
    public int CarteCompetenceId { get; set; }
    public string CarteCode { get; set; } = string.Empty;
    public string CarteTitreTheorie { get; set; } = string.Empty;
    public string UtilisateurId { get; set; } = string.Empty;
    public string UtilisateurNomComplet { get; set; } = string.Empty;
    public string AttribueParId { get; set; } = string.Empty;
    public string AttribueParNomComplet { get; set; } = string.Empty;
    public DateTime AttribueLe { get; set; }
    public string? Contexte { get; set; }
    public bool EstActif { get; set; }
}

public interface ICarteCompetenceService
{
    Task<CarteCompetenceResultatRecherche> RechercherAsync(CarteCompetenceFiltre filtre);

    Task<CarteCompetence?> GetByIdAsync(int id);

    Task<List<Badge>> GetBadgesAsync();

    Task<(bool Success, string? ErrorMessage, CarteCompetence? Carte)> CreateAsync(CarteCompetenceInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, CarteCompetenceInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);

    // Import/synchronisation depuis un fichier .xlsx conforme a data.xlsx (feuilles
    // "badges" et "cartes"). Upsert uniquement : ne supprime jamais une carte absente du
    // fichier. Ne touche jamais aux attributions (action manuelle separee).
    Task<ImportRapport> ImporterAsync(Stream fichierXlsx);

    // ---- Attribution (cote gestionnaire) ----

    Task<List<CarteAttributionInfo>> GetAttributionsPourCarteAsync(int carteId);

    Task<List<CarteAttributionInfo>> GetAttributionsPourUtilisateurAsync(string utilisateurId);

    Task<(bool Success, string? ErrorMessage)> AttribuerAsync(
        List<int> carteIds,
        List<string> utilisateurIds,
        string attribueParId,
        string? contexte);

    Task<(bool Success, string? ErrorMessage)> DesattribuerAsync(int attributionId);
}
