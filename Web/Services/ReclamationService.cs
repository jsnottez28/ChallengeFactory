using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalServices.Email;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class ReclamationService(ApplicationDbContext dbContext, IEmailService emailService) : IReclamationService
{
    public async Task DeposerAsync(ReclamationInput input)
    {
        dbContext.Reclamations.Add(new Reclamation
        {
            Nom = input.Nom.Trim(),
            Email = input.Email.Trim(),
            Message = input.Message.Trim(),
            UtilisateurId = input.UtilisateurId,
        });
        await dbContext.SaveChangesAsync();

        await emailService.EnvoyerAsync(input.Email.Trim(), "Réclamation bien reçue", EmailTemplates.AccuseReceptionReclamation(input.Nom));
    }

    public async Task<List<ReclamationInfo>> GetAllAsync()
    {
        var reclamations = await dbContext.Reclamations
            .Include(r => r.TraitePar)
            .OrderByDescending(r => r.DeposeLe)
            .ToListAsync();

        return reclamations.Select(VersInfo).ToList();
    }

    public async Task<ReclamationInfo?> GetByIdAsync(int id)
    {
        var reclamation = await dbContext.Reclamations
            .Include(r => r.TraitePar)
            .FirstOrDefaultAsync(r => r.Id == id);

        return reclamation is null ? null : VersInfo(reclamation);
    }

    public async Task<(bool Success, string? ErrorMessage)> PrendreEnChargeAsync(int id, string gestionnaireId)
    {
        var reclamation = await dbContext.Reclamations.FirstOrDefaultAsync(r => r.Id == id);
        if (reclamation is null)
        {
            return (false, "Réclamation introuvable.");
        }

        if (reclamation.Statut == StatutReclamation.Traitee)
        {
            return (false, "Cette réclamation est déjà traitée.");
        }

        reclamation.Statut = StatutReclamation.EnCours;
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RepondreAsync(int id, string gestionnaireId, string reponse)
    {
        if (string.IsNullOrWhiteSpace(reponse))
        {
            return (false, "La réponse ne peut pas être vide.");
        }

        var reclamation = await dbContext.Reclamations.FirstOrDefaultAsync(r => r.Id == id);
        if (reclamation is null)
        {
            return (false, "Réclamation introuvable.");
        }

        reclamation.Reponse = reponse.Trim();
        reclamation.Statut = StatutReclamation.Traitee;
        reclamation.TraiteParId = gestionnaireId;
        reclamation.TraiteLe = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        await emailService.EnvoyerAsync(reclamation.Email, "Réponse à votre réclamation", EmailTemplates.ReponseReclamation(reclamation.Reponse));

        return (true, null);
    }

    private static ReclamationInfo VersInfo(Reclamation r) => new()
    {
        Id = r.Id,
        Nom = r.Nom,
        Email = r.Email,
        Message = r.Message,
        DeposeLe = r.DeposeLe,
        Statut = r.Statut,
        Reponse = r.Reponse,
        TraiteParNomComplet = r.TraitePar is null ? null : NomComplet(r.TraitePar),
        TraiteLe = r.TraiteLe,
    };

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}
