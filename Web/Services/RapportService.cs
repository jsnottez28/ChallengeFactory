using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Domain.Entities;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed partial class RapportService(ApplicationDbContext dbContext) : IRapportService
{
    // DisableHtml() : toute balise HTML presente dans le Markdown source est echappee
    // (affichee comme texte) au lieu d'etre inseree telle quelle dans la page - meme
    // pipeline que ArticleService.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public async Task<RapportResultatRecherche> RechercherAdminAsync(int page, int taillePage)
    {
        var requete = dbContext.Rapports
            .OrderByDescending(r => r.DatePublication)
            .AsQueryable();

        var total = await requete.CountAsync();
        var rapports = await requete
            .Skip((Math.Max(page, 1) - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync();

        return new RapportResultatRecherche { Rapports = rapports, NombreTotal = total };
    }

    public async Task<Rapport?> GetByIdAsync(int id) =>
        await dbContext.Rapports.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<(bool Success, string? ErrorMessage, Rapport? Rapport)> CreateAsync(RapportInput input)
    {
        var erreur = await ValiderAsync(input, rapportIdExclu: null);
        if (erreur is not null)
        {
            return (false, erreur, null);
        }

        var rapport = new Rapport();
        AppliquerInput(rapport, input);

        dbContext.Rapports.Add(rapport);
        await dbContext.SaveChangesAsync();

        return (true, null, rapport);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, RapportInput input)
    {
        var rapport = await dbContext.Rapports.FirstOrDefaultAsync(r => r.Id == id);
        if (rapport is null)
        {
            return (false, "Rapport introuvable.");
        }

        var erreur = await ValiderAsync(input, rapportIdExclu: id);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        AppliquerInput(rapport, input);
        rapport.ModifieLe = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var rapport = await dbContext.Rapports.FirstOrDefaultAsync(r => r.Id == id);
        if (rapport is null)
        {
            return (false, "Rapport introuvable.");
        }

        dbContext.Rapports.Remove(rapport);
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

    public string DeterminerStatutAffiche(Rapport rapport) => rapport.Statut switch
    {
        StatutRapport.Brouillon => "Brouillon",
        StatutRapport.Publie when rapport.DatePublication > DateTime.UtcNow => "Programmé",
        StatutRapport.Publie => "Publié",
        _ => rapport.Statut.ToString(),
    };

    public async Task<(List<Rapport> Rapports, int NombreTotal)> GetPublicsAsync(int page, int taillePage)
    {
        var requete = RequetePublique();

        var total = await requete.CountAsync();
        var rapports = await requete
            .Skip((Math.Max(page, 1) - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync();

        return (rapports, total);
    }

    public async Task<Rapport?> GetPublicParSlugAsync(string slug) =>
        await RequetePublique().FirstOrDefaultAsync(r => r.Slug == slug);

    public async Task<List<Rapport>> GetDerniersPublicsAsync(int nombre) =>
        await RequetePublique().Take(nombre).ToListAsync();

    public async Task<Rapport?> GetParSlugAsync(string slug) =>
        await dbContext.Rapports.FirstOrDefaultAsync(r => r.Slug == slug);

    public string RendreHtml(string markdown) =>
        string.IsNullOrWhiteSpace(markdown) ? string.Empty : Markdown.ToHtml(markdown, Pipeline);

    private IOrderedQueryable<Rapport> RequetePublique() =>
        dbContext.Rapports
            .Where(r => r.Statut == StatutRapport.Publie && r.DatePublication <= DateTime.UtcNow)
            .OrderByDescending(r => r.DatePublication);

    private async Task<string?> ValiderAsync(RapportInput input, int? rapportIdExclu)
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
            return "Le slug ne doit contenir que des minuscules, des chiffres et des tirets (ex. mon-rapport).";
        }

        var slugExiste = await dbContext.Rapports
            .AnyAsync(r => r.Slug == input.Slug && (rapportIdExclu == null || r.Id != rapportIdExclu));
        if (slugExiste)
        {
            return "Un rapport avec ce slug existe déjà.";
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

    private static void AppliquerInput(Rapport rapport, RapportInput input)
    {
        rapport.Titre = input.Titre.Trim();
        rapport.Slug = input.Slug.Trim();
        rapport.Resume = input.Resume.Trim();
        rapport.MetaDescription = input.MetaDescription.Trim();
        rapport.ContenuMarkdown = input.ContenuMarkdown;
        rapport.Type = input.Type;
        rapport.Auteur = input.Auteur.Trim();
        rapport.DatePublication = input.DatePublication;
        rapport.Statut = input.Statut;

        if (input.ImageCouverture is not null)
        {
            rapport.ImageCouverture = input.ImageCouverture;
        }

        if (input.FichierPdf is not null)
        {
            rapport.FichierPdf = input.FichierPdf;
        }
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex CaracteresNonAlphanumeriques();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex TiretsConsecutifs();

    [GeneratedRegex(@"^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugValide();
}
