using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Domain.Entities;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed partial class ArticleService(ApplicationDbContext dbContext) : IArticleService
{
    // DisableHtml() : toute balise HTML presente dans le Markdown source est echappee
    // (affichee comme texte) au lieu d'etre inseree telle quelle dans la page - c'est ce
    // qui rend le rendu "sur" sans necessiter de librairie de sanitisation HTML
    // supplementaire (cf. demande "HTML nettoye").
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public async Task<ArticleResultatRecherche> RechercherAdminAsync(int page, int taillePage)
    {
        var requete = dbContext.Articles
            .OrderByDescending(a => a.DatePublication)
            .AsQueryable();

        var total = await requete.CountAsync();
        var articles = await requete
            .Skip((Math.Max(page, 1) - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync();

        return new ArticleResultatRecherche { Articles = articles, NombreTotal = total };
    }

    public async Task<Article?> GetByIdAsync(int id) =>
        await dbContext.Articles
            .Include(a => a.Sources.OrderBy(s => s.Ordre))
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<(bool Success, string? ErrorMessage, Article? Article)> CreateAsync(ArticleInput input)
    {
        var erreur = await ValiderAsync(input, articleIdExclu: null);
        if (erreur is not null)
        {
            return (false, erreur, null);
        }

        var article = new Article();
        AppliquerInput(article, input);

        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync();

        return (true, null, article);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, ArticleInput input)
    {
        var article = await dbContext.Articles
            .Include(a => a.Sources)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (article is null)
        {
            return (false, "Article introuvable.");
        }

        var erreur = await ValiderAsync(input, articleIdExclu: id);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        AppliquerInput(article, input);
        article.ModifieLe = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var article = await dbContext.Articles.FirstOrDefaultAsync(a => a.Id == id);
        if (article is null)
        {
            return (false, "Article introuvable.");
        }

        dbContext.Articles.Remove(article);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public string GenererSlug(string titre)
    {
        if (string.IsNullOrWhiteSpace(titre))
        {
            return string.Empty;
        }

        // Retire les accents (decomposition Unicode puis filtrage des marques
        // diacritiques), passe en minuscules, remplace tout ce qui n'est pas
        // alphanumerique par un tiret, puis compacte/trim les tirets.
        var normalise = titre.Normalize(NormalizationForm.FormD);
        var sansAccents = new StringBuilder();
        foreach (var caractere in normalise)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                sansAccents.Append(caractere);
            }
        }

        var minuscule = sansAccents.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var slug = CaracteresNonAlphanumeriques().Replace(minuscule, "-");
        slug = TiretsConsecutifs().Replace(slug, "-").Trim('-');

        return slug;
    }

    public string DeterminerStatutAffiche(Article article) => article.Statut switch
    {
        StatutArticle.Brouillon => "Brouillon",
        StatutArticle.Publie when article.DatePublication > DateTime.UtcNow => "Programmé",
        StatutArticle.Publie => "Publié",
        _ => article.Statut.ToString(),
    };

    public async Task<(List<Article> Articles, int NombreTotal)> GetPublicsAsync(int page, int taillePage)
    {
        var requete = RequetePublique();

        var total = await requete.CountAsync();
        var articles = await requete
            .Skip((Math.Max(page, 1) - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync();

        return (articles, total);
    }

    public async Task<Article?> GetPublicParSlugAsync(string slug) =>
        await RequetePublique()
            .Include(a => a.Sources.OrderBy(s => s.Ordre))
            .FirstOrDefaultAsync(a => a.Slug == slug);

    public async Task<List<Article>> GetDerniersPublicsAsync(int nombre) =>
        await RequetePublique().Take(nombre).ToListAsync();

    public async Task<Article?> GetParSlugAsync(string slug) =>
        await dbContext.Articles
            .Include(a => a.Sources.OrderBy(s => s.Ordre))
            .FirstOrDefaultAsync(a => a.Slug == slug);

    public string RendreHtml(string markdown) =>
        string.IsNullOrWhiteSpace(markdown) ? string.Empty : Markdown.ToHtml(markdown, Pipeline);

    private IOrderedQueryable<Article> RequetePublique() =>
        dbContext.Articles
            .Where(a => a.Statut == StatutArticle.Publie && a.DatePublication <= DateTime.UtcNow)
            .OrderByDescending(a => a.DatePublication);

    private async Task<string?> ValiderAsync(ArticleInput input, int? articleIdExclu)
    {
        if (string.IsNullOrWhiteSpace(input.Titre))
        {
            return "Le titre est obligatoire.";
        }

        if (string.IsNullOrWhiteSpace(input.Slug))
        {
            return "Le slug est obligatoire.";
        }

        if (!SlugValide().IsMatch(input.Slug))
        {
            return "Le slug ne doit contenir que des minuscules, des chiffres et des tirets (ex. mon-article).";
        }

        if (input.Slug == "rss")
        {
            return "« rss » est réservé au flux RSS, choisissez un autre slug.";
        }

        var slugExiste = await dbContext.Articles
            .AnyAsync(a => a.Slug == input.Slug && (articleIdExclu == null || a.Id != articleIdExclu));
        if (slugExiste)
        {
            return "Un article avec ce slug existe déjà.";
        }

        if (string.IsNullOrWhiteSpace(input.Resume))
        {
            return "Le résumé est obligatoire.";
        }

        if (string.IsNullOrWhiteSpace(input.MetaDescription))
        {
            return "La meta description est obligatoire.";
        }

        if (input.MetaDescription.Length > 160)
        {
            return "La meta description ne doit pas dépasser 160 caractères.";
        }

        if (string.IsNullOrWhiteSpace(input.ContenuMarkdown))
        {
            return "Le contenu est obligatoire.";
        }

        if (string.IsNullOrWhiteSpace(input.Auteur))
        {
            return "L'auteur est obligatoire.";
        }

        return null;
    }

    private static void AppliquerInput(Article article, ArticleInput input)
    {
        article.Titre = input.Titre.Trim();
        article.Slug = input.Slug.Trim();
        article.Resume = input.Resume.Trim();
        article.MetaDescription = input.MetaDescription.Trim();
        article.ContenuMarkdown = input.ContenuMarkdown;
        article.Auteur = input.Auteur.Trim();
        article.DatePublication = input.DatePublication;
        article.Statut = input.Statut;

        if (input.ImageCouverture is not null)
        {
            article.ImageCouverture = input.ImageCouverture;
        }

        article.Sources.Clear();
        var ordre = 0;
        foreach (var source in input.Sources)
        {
            if (string.IsNullOrWhiteSpace(source.Url))
            {
                continue;
            }

            article.Sources.Add(new ArticleSource
            {
                Libelle = string.IsNullOrWhiteSpace(source.Libelle) ? null : source.Libelle.Trim(),
                Url = source.Url.Trim(),
                Ordre = ordre++,
            });
        }
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex CaracteresNonAlphanumeriques();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex TiretsConsecutifs();

    [GeneratedRegex(@"^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugValide();
}
