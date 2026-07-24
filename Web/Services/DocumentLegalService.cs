using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class DocumentLegalService(ApplicationDbContext dbContext) : IDocumentLegalService
{
    public async Task<List<DocumentLegal>> GetHistoriqueAsync(TypeDocumentLegal type)
    {
        return await dbContext.DocumentsLegaux
            .Where(document => document.Type == type)
            .OrderByDescending(document => document.DateEffective)
            .ToListAsync();
    }

    public async Task<DocumentLegal?> GetVersionPublieeAsync(TypeDocumentLegal type)
    {
        return await dbContext.DocumentsLegaux
            .Where(document => document.Type == type && document.EstPublie)
            .OrderByDescending(document => document.DateEffective)
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string? ErrorMessage, DocumentLegal? Document)> CreerEtPublierAsync(DocumentLegalInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Version))
        {
            return (false, "La version est obligatoire.", null);
        }

        if (string.IsNullOrWhiteSpace(input.Contenu))
        {
            return (false, "Le contenu est obligatoire.", null);
        }

        var versionExistante = await dbContext.DocumentsLegaux
            .AnyAsync(document => document.Type == input.Type && document.Version == input.Version);
        if (versionExistante)
        {
            return (false, "Cette version existe deja pour ce type de document.", null);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var anciennesVersionsPublieees = dbContext.DocumentsLegaux
            .Where(document => document.Type == input.Type && document.EstPublie);
        await anciennesVersionsPublieees.ForEachAsync(document => document.EstPublie = false);

        var nouvelleVersion = new DocumentLegal
        {
            Type = input.Type,
            Version = input.Version,
            Contenu = input.Contenu,
            DateEffective = input.DateEffective,
            EstPublie = true,
        };

        dbContext.DocumentsLegaux.Add(nouvelleVersion);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null, nouvelleVersion);
    }

    public async Task<List<TypeDocumentLegal>> GetTypesEnAttenteAsync(string applicationUserId)
    {
        var typesEnAttente = new List<TypeDocumentLegal>();

        foreach (var type in Enum.GetValues<TypeDocumentLegal>())
        {
            var versionPubliee = await GetVersionPublieeAsync(type);
            if (versionPubliee is null)
            {
                continue;
            }

            var dejaAcceptee = await dbContext.AcceptationsDocumentsLegaux
                .AnyAsync(acceptation =>
                    acceptation.ApplicationUserId == applicationUserId &&
                    acceptation.DocumentLegalId == versionPubliee.Id);

            if (!dejaAcceptee)
            {
                typesEnAttente.Add(type);
            }
        }

        return typesEnAttente;
    }

    public async Task<List<TypeDocumentLegal>> GetTypesBloquantsAsync(string applicationUserId, DateTimeOffset sessionDebutUtc)
    {
        var typesEnAttente = await GetTypesEnAttenteAsync(applicationUserId);
        if (typesEnAttente.Count == 0)
        {
            return [];
        }

        var typesBloquants = new List<TypeDocumentLegal>();
        foreach (var type in typesEnAttente)
        {
            var versionPubliee = await GetVersionPublieeAsync(type);
            if (versionPubliee is not null && sessionDebutUtc >= versionPubliee.DateEffective)
            {
                typesBloquants.Add(type);
            }
        }

        return typesBloquants;
    }

    public async Task<(bool Success, string? ErrorMessage)> AccepterAsync(string applicationUserId, TypeDocumentLegal type, string? adresseIp)
    {
        var versionPubliee = await GetVersionPublieeAsync(type);
        if (versionPubliee is null)
        {
            return (false, "Aucune version publiee pour ce type de document.");
        }

        var dejaAcceptee = await dbContext.AcceptationsDocumentsLegaux
            .AnyAsync(acceptation =>
                acceptation.ApplicationUserId == applicationUserId &&
                acceptation.DocumentLegalId == versionPubliee.Id);
        if (dejaAcceptee)
        {
            return (true, null);
        }

        dbContext.AcceptationsDocumentsLegaux.Add(new AcceptationDocumentLegal
        {
            ApplicationUserId = applicationUserId,
            DocumentLegalId = versionPubliee.Id,
            AdresseIp = adresseIp,
        });

        await dbContext.SaveChangesAsync();

        return (true, null);
    }
}
