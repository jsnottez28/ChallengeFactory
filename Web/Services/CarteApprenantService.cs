using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class CarteApprenantService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : ICarteApprenantService
{
    public async Task<List<CarteBibliothequeInfo>> GetMesCartesAsync(string utilisateurId)
    {
        if (!await AccesContenuAutoriseAsync(utilisateurId))
        {
            return [];
        }

        var attributions = await dbContext.CarteAttributions
            .Where(a => a.UtilisateurId == utilisateurId && a.EstActif)
            .Include(a => a.CarteCompetence)
                .ThenInclude(c => c.Badge)
            .Include(a => a.ChallengeEtape)
                .ThenInclude(e => e!.Challenge)
            .OrderByDescending(a => a.AttribueLe)
            .ToListAsync();

        return attributions.Select(a => new CarteBibliothequeInfo
        {
            Carte = a.CarteCompetence,
            OrigineType = a.OrigineType,
            ChallengeTitre = a.ChallengeEtape?.Challenge.Titre,
            NumeroEtape = a.ChallengeEtape?.NumeroEtape,
            AttribueLe = a.AttribueLe,
        }).ToList();
    }

    public async Task<CarteCompetence?> GetCarteAttribueeAsync(string utilisateurId, int carteId)
    {
        if (!await AccesContenuAutoriseAsync(utilisateurId))
        {
            return null;
        }

        // La jointure sur CarteAttribution.EstActif est ce qui empeche un apprenant
        // d'acceder a une carte en devinant/forcant son Id dans l'URL : sans ligne
        // d'attribution active, aucune carte n'est renvoyee, quel que soit l'Id demande.
        return await dbContext.CarteAttributions
            .Where(a => a.UtilisateurId == utilisateurId && a.EstActif && a.CarteCompetenceId == carteId)
            .Include(a => a.CarteCompetence)
                .ThenInclude(c => c.Badge)
            .Select(a => a.CarteCompetence)
            .FirstOrDefaultAsync();
    }

    // Un compte Suspendu ou En attente de validation (BtoC, statut_acces_plateforme) ne
    // doit avoir acces a aucun contenu apprenant - meme s'il a deja des cartes attribuees
    // par ailleurs. Verifie cote serveur, jamais seulement masque cote UI.
    private async Task<bool> AccesContenuAutoriseAsync(string utilisateurId)
    {
        var utilisateur = await userManager.FindByIdAsync(utilisateurId);
        return utilisateur is not null && (utilisateur.EstSuperAdministrateur || utilisateur.Statut == StatutUtilisateur.Actif);
    }

    // ---- Notes personnelles sur une carte ----

    public async Task<List<CommentaireCarteInfo>> GetMesCommentairesAsync(string utilisateurId, int carteId)
    {
        if (!await AccesContenuAutoriseAsync(utilisateurId))
        {
            return [];
        }

        var commentaires = await dbContext.CommentairesCarte
            .Where(c => c.UtilisateurId == utilisateurId && c.CarteCompetenceId == carteId)
            .OrderBy(c => c.DateCreation)
            .ToListAsync();

        return commentaires.Select(c => new CommentaireCarteInfo
        {
            Id = c.Id,
            Contenu = c.Contenu,
            DateCreation = c.DateCreation,
            DateModification = c.DateModification,
        }).ToList();
    }

    public async Task<(bool Success, string? ErrorMessage, int? CommentaireId)> AjouterCommentaireAsync(string utilisateurId, int carteId, string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
        {
            return (false, "Le commentaire ne peut pas être vide.", null);
        }

        // Meme controle d'acces que GetCarteAttribueeAsync : pas de note possible sur une
        // carte qui n'est pas (ou plus) attribuee a cet utilisateur.
        if (await GetCarteAttribueeAsync(utilisateurId, carteId) is null)
        {
            return (false, "Cette carte ne vous est pas attribuée.", null);
        }

        var commentaire = new CommentaireCarte
        {
            UtilisateurId = utilisateurId,
            CarteCompetenceId = carteId,
            Contenu = contenu.Trim(),
        };

        dbContext.CommentairesCarte.Add(commentaire);
        await dbContext.SaveChangesAsync();

        return (true, null, commentaire.Id);
    }

    public async Task<(bool Success, string? ErrorMessage)> ModifierCommentaireAsync(int commentaireId, string utilisateurId, string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
        {
            return (false, "Le commentaire ne peut pas être vide.");
        }

        var commentaire = await dbContext.CommentairesCarte.FirstOrDefaultAsync(c => c.Id == commentaireId);
        if (commentaire is null || commentaire.UtilisateurId != utilisateurId)
        {
            return (false, "Commentaire introuvable.");
        }

        commentaire.Contenu = contenu.Trim();
        commentaire.DateModification = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> SupprimerCommentaireAsync(int commentaireId, string utilisateurId)
    {
        var commentaire = await dbContext.CommentairesCarte.FirstOrDefaultAsync(c => c.Id == commentaireId);
        if (commentaire is null || commentaire.UtilisateurId != utilisateurId)
        {
            return (false, "Commentaire introuvable.");
        }

        dbContext.CommentairesCarte.Remove(commentaire);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }
}
