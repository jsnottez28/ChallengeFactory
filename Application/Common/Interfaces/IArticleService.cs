using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class ArticleSourceInput
{
    public string? Libelle { get; set; }
    public string Url { get; set; } = string.Empty;
}

public sealed class ArticleInput
{
    public string Titre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string ContenuMarkdown { get; set; } = string.Empty;

    // Nom de fichier deja enregistre sur disque (l'upload lui-meme est gere par le
    // controleur, pas par le service - meme principe que CarteCompetenceInput.ImageCarteA).
    // Null = pas de changement d'image (conserve l'image actuelle sur UpdateAsync).
    public string? ImageCouverture { get; set; }

    public string Auteur { get; set; } = string.Empty;
    public DateTime DatePublication { get; set; }
    public StatutArticle Statut { get; set; }
    public List<ArticleSourceInput> Sources { get; set; } = [];
}

public sealed class ArticleResultatRecherche
{
    public List<Article> Articles { get; set; } = [];
    public int NombreTotal { get; set; }
}

// Blog public (/articles) + back-office (/admin/articles). Le Markdown saisi en admin
// n'est jamais stocke en HTML : il est rendu a l'affichage via RendreHtml (pipeline
// Markdig avec DisableHtml(), pour ne jamais laisser passer de balise brute saisie dans
// le Markdown source - jamais un QCM/validation de competence, hors du champ du principe
// Manifeste sur le QCM).
public interface IArticleService
{
    // ---- Back-office ----

    Task<ArticleResultatRecherche> RechercherAdminAsync(int page, int taillePage);

    Task<Article?> GetByIdAsync(int id);

    Task<(bool Success, string? ErrorMessage, Article? Article)> CreateAsync(ArticleInput input);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, ArticleInput input);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);

    // Genere un slug a partir du titre (minuscules, accents retires, espaces/ponctuation
    // remplaces par des tirets) - point de depart propose en admin, toujours modifiable.
    string GenererSlug(string titre);

    // "Brouillon", "Programmé" ou "Publié" - libelle affiche dans la liste admin, derive
    // de Statut + DatePublication (cf. StatutArticle, aucun 3e etat persiste).
    string DeterminerStatutAffiche(Article article);

    // ---- Public ----

    // Articles effectivement visibles (Statut = Publie ET DatePublication <= maintenant),
    // du plus recent au plus ancien.
    Task<(List<Article> Articles, int NombreTotal)> GetPublicsAsync(int page, int taillePage);

    Task<Article?> GetPublicParSlugAsync(string slug);

    // N'importe quel article par slug, quel que soit son statut (y compris Brouillon ou
    // Programme) - reserve a l'apercu admin (cf. ArticlesController.Details, qui verifie
    // lui-meme le Droit ARTICLE.CONSULTER avant d'appeler cette methode).
    Task<Article?> GetParSlugAsync(string slug);

    // Les N articles publics les plus recents, pour le flux RSS.
    Task<List<Article>> GetDerniersPublicsAsync(int nombre);

    // Rendu HTML sur, a partir du Markdown stocke - jamais de HTML brut passe tel quel
    // (DisableHtml() sur le pipeline Markdig).
    string RendreHtml(string markdown);
}
