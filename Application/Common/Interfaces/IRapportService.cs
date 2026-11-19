using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class RapportInput
{
    public string Titre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string ContenuMarkdown { get; set; } = string.Empty;
    public TypeRapport Type { get; set; }

    // Nom de fichier deja enregistre sur disque (l'upload lui-meme est gere par le
    // controleur, pas par le service - meme principe que ArticleInput.ImageCouverture).
    // Null = pas de changement d'image (conserve l'image actuelle sur UpdateAsync).
    public string? ImageCouverture { get; set; }

    // Idem pour le PDF de l'etude complete : null = pas de changement.
    public string? FichierPdf { get; set; }

    public string Auteur { get; set; } = string.Empty;
    public DateTime DatePublication { get; set; }
    public StatutRapport Statut { get; set; }
}

public sealed class RapportResultatRecherche
{
    public List<Rapport> Rapports { get; set; } = [];
    public int NombreTotal { get; set; }
}

// Espace public de publication des etudes de cas Bilan Carbone(R)/ACV realisees
// (/rapports) + back-office (/admin/rapports) - miroir exact d'IArticleService, sans la
// notion de Sources (pas de source externe a citer pour une etude de cas).
public interface IRapportService
{
    // ---- Back-office ----

    Task<RapportResultatRecherche> RechercherAdminAsync(int page, int taillePage);

    Task<Rapport?> GetByIdAsync(int id);

    Task<(bool Success, string? ErrorMessage, Rapport? Rapport)> CreateAsync(RapportInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, RapportInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);

    // Genere un slug a partir du titre (minuscules, accents retires, espaces/ponctuation
    // remplaces par des tirets) - point de depart propose en admin, toujours modifiable.
    string GenererSlug(string titre);

    // "Brouillon", "Programmé" ou "Publié" - libelle affiche dans la liste admin, derive
    // de Statut + DatePublication (cf. StatutRapport, aucun 3e etat persiste).
    string DeterminerStatutAffiche(Rapport rapport);

    // ---- Public ----

    // Rapports effectivement visibles (Statut = Publie ET DatePublication <= maintenant),
    // du plus recent au plus ancien.
    Task<(List<Rapport> Rapports, int NombreTotal)> GetPublicsAsync(int page, int taillePage);

    Task<Rapport?> GetPublicParSlugAsync(string slug);

    // N'importe quel rapport par slug, quel que soit son statut (y compris Brouillon ou
    // Programme) - reserve a l'apercu admin (cf. RapportsController.Details, qui verifie
    // lui-meme le Droit RAPPORT.CONSULTER avant d'appeler cette methode).
    Task<Rapport?> GetParSlugAsync(string slug);

    // Les N rapports publics les plus recents, pour le sitemap.
    Task<List<Rapport>> GetDerniersPublicsAsync(int nombre);

    // Rendu HTML sur, a partir du Markdown stocke - jamais de HTML brut passe tel quel
    // (DisableHtml() sur le pipeline Markdig).
    string RendreHtml(string markdown);
}
