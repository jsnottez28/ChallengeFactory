using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class ForumService(ApplicationDbContext dbContext, INotificationService notificationService) : IForumService
{
    public async Task<List<EtapeForumInfo>> GetEtapesAccessiblesAsync(int cohorteId, string utilisateurId, bool estAdmin = false)
    {
        if (!estAdmin && !await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId))
        {
            return [];
        }

        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null || cohorte.EtapeCourante < 1)
        {
            return [];
        }

        var etapes = await dbContext.ChallengeEtapes
            .Where(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape <= cohorte.EtapeCourante)
            .OrderBy(e => e.NumeroEtape)
            .ToListAsync();

        return etapes.Select(e => new EtapeForumInfo
        {
            ChallengeEtapeId = e.Id,
            NumeroEtape = e.NumeroEtape,
            TitreEtape = e.TitreEtape,
            EstEtapeCourante = e.NumeroEtape == cohorte.EtapeCourante,
        }).ToList();
    }

    public async Task<List<ForumMessageInfo>> GetMessagesEtapeAsync(int challengeEtapeId, int cohorteId, string utilisateurId, bool estAdmin = false)
    {
        if (!estAdmin && !await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId))
        {
            return [];
        }

        var messages = await dbContext.ForumMessages
            .Include(m => m.Auteur)
            .Include(m => m.MarquagesUtile)
            .Where(m => m.CohorteId == cohorteId && m.ChallengeEtapeId == challengeEtapeId)
            .OrderBy(m => m.DateCreation)
            .ToListAsync();

        var parId = messages.ToDictionary(m => m.Id, m => new ForumMessageInfo
        {
            Id = m.Id,
            AuteurNomComplet = NomComplet(m.Auteur),
            EstAuteur = m.AuteurId == utilisateurId,
            Contenu = m.Contenu,
            DateCreation = m.DateCreation,
            NombreUtile = m.MarquagesUtile.Count,
            MarqueUtileParMoi = m.MarquagesUtile.Any(u => u.MarqueParId == utilisateurId),
        });

        var racines = new List<ForumMessageInfo>();
        foreach (var message in messages)
        {
            var info = parId[message.Id];
            if (message.MessageParentId is int parentId && parId.TryGetValue(parentId, out var parent))
            {
                parent.Reponses.Add(info);
            }
            else
            {
                racines.Add(info);
            }
        }

        return racines;
    }

    public async Task<(bool Success, string? ErrorMessage)> PosterMessageAsync(
        string auteurId, int cohorteId, int challengeEtapeId, string contenu, int? messageParentId, string lienForum)
    {
        if (string.IsNullOrWhiteSpace(contenu))
        {
            return (false, "Le message ne peut pas être vide.");
        }

        if (!await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == auteurId))
        {
            return (false, "Vous n'êtes pas membre de cette Cohorte.");
        }

        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        var etape = await dbContext.ChallengeEtapes.FirstOrDefaultAsync(e => e.Id == challengeEtapeId);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active || etape is null || etape.ChallengeId != cohorte.ChallengeId || etape.NumeroEtape != cohorte.EtapeCourante)
        {
            return (false, "Ce forum est celui d'une étape déjà clôturée : lecture seule.");
        }

        ForumMessage? messageParent = null;
        if (messageParentId is int parentId)
        {
            messageParent = await dbContext.ForumMessages.FirstOrDefaultAsync(m => m.Id == parentId && m.CohorteId == cohorteId && m.ChallengeEtapeId == challengeEtapeId);
            if (messageParent is null)
            {
                return (false, "Message parent introuvable.");
            }
        }

        var message = new ForumMessage
        {
            CohorteId = cohorteId,
            ChallengeEtapeId = challengeEtapeId,
            AuteurId = auteurId,
            Contenu = contenu.Trim(),
            MessageParentId = messageParentId,
        };
        dbContext.ForumMessages.Add(message);
        await dbContext.SaveChangesAsync();

        if (messageParent is not null)
        {
            // Reponse en fil : uniquement l'auteur du message parent, notification
            // specifique - pas la notification generique "nouveau message" en plus (evite
            // une double notification pour le meme evenement, cf. resume de livraison).
            if (messageParent.AuteurId != auteurId)
            {
                await notificationService.CreerAsync(messageParent.AuteurId, TypeNotification.ReponseAMonMessage,
                    ReferenceTypeNotification.ForumMessage, message.Id,
                    "Quelqu'un a répondu à ton message — viens lire sa réponse", lienForum);
            }
        }
        else
        {
            var autresMembres = await dbContext.CohorteMembres
                .Where(m => m.CohorteId == cohorteId && m.UtilisateurId != auteurId)
                .Select(m => m.UtilisateurId)
                .ToListAsync();

            foreach (var destinataireId in autresMembres)
            {
                await notificationService.CreerAsync(destinataireId, TypeNotification.NouveauMessageForum,
                    ReferenceTypeNotification.ForumMessage, message.Id,
                    "Nouveau message dans le forum de ton étape — viens voir !", lienForum);
            }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> MarquerUtileAsync(int messageId, string marqueParId, string lienForum)
    {
        var message = await dbContext.ForumMessages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message is null)
        {
            return (false, "Message introuvable.");
        }

        if (message.AuteurId == marqueParId)
        {
            return (false, "Vous ne pouvez pas marquer votre propre message comme utile.");
        }

        if (!await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == message.CohorteId && m.UtilisateurId == marqueParId))
        {
            return (false, "Vous n'êtes pas membre de cette Cohorte.");
        }

        if (await dbContext.ForumMessagesUtiles.AnyAsync(u => u.MessageId == messageId && u.MarqueParId == marqueParId))
        {
            return (false, "Vous avez déjà marqué ce message comme utile.");
        }

        var marquage = new ForumMessageUtile { MessageId = messageId, MarqueParId = marqueParId };
        dbContext.ForumMessagesUtiles.Add(marquage);
        await dbContext.SaveChangesAsync();

        dbContext.PointsEvenements.Add(new PointsEvenement
        {
            UtilisateurId = message.AuteurId,
            CohorteId = message.CohorteId,
            TypePoints = TypePoints.PointsKarma,
            Montant = PointsConfig.PointsKarmaMessageUtile,
            Motif = MotifPoints.MessageForumUtile,
            ReferenceType = ReferenceTypePoints.ForumMessageUtile,
            ReferenceId = marquage.Id,
            Date = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        await notificationService.CreerAsync(message.AuteurId, TypeNotification.MessageMarqueUtile,
            ReferenceTypeNotification.ForumMessage, message.Id,
            "Un pair a trouvé ton message utile — bravo pour ta contribution !", lienForum);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> SupprimerMessageAsync(int messageId)
    {
        var message = await dbContext.ForumMessages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message is null)
        {
            return (false, "Message introuvable.");
        }

        // La FK MessageParentId est en Restrict (pas Cascade - SQL Server refuse un
        // cascade auto-reference), donc la suppression des reponses en fil est geree ici :
        // il faut supprimer les descendants avant le message lui-meme.
        var aSupprimer = new List<ForumMessage> { message };
        var frontiere = new List<int> { messageId };
        while (frontiere.Count > 0)
        {
            var enfants = await dbContext.ForumMessages.Where(m => m.MessageParentId != null && frontiere.Contains(m.MessageParentId.Value)).ToListAsync();
            if (enfants.Count == 0)
            {
                break;
            }

            aSupprimer.AddRange(enfants);
            frontiere = enfants.Select(m => m.Id).ToList();
        }

        dbContext.ForumMessages.RemoveRange(aSupprimer);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}
