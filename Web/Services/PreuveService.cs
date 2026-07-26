using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class PreuveService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IPreuveFichierStockageService stockageService,
    INotificationService notificationService,
    IEmailService emailService) : IPreuveService
{
    // Hypothese posee dans le prompt d'origine, a confirmer/ajuster selon la capacite de
    // stockage reelle retenue (cf. resume de livraison).
    private const long TailleMaxFichierOctets = 50L * 1024 * 1024;

    private static readonly Dictionary<string, TypeFichierPreuve> ExtensionsAutorisees = new(StringComparer.OrdinalIgnoreCase)
    {
        [".doc"] = TypeFichierPreuve.Document,
        [".docx"] = TypeFichierPreuve.Document,
        [".pdf"] = TypeFichierPreuve.Document,
        [".jpg"] = TypeFichierPreuve.Image,
        [".jpeg"] = TypeFichierPreuve.Image,
        [".png"] = TypeFichierPreuve.Image,
        [".webp"] = TypeFichierPreuve.Image,
        [".mp4"] = TypeFichierPreuve.Video,
        [".mov"] = TypeFichierPreuve.Video,
    };

    // ---- Depot / modification ----

    public async Task<(bool Success, string? ErrorMessage, int? PreuveId)> DeposerOuModifierAsync(
        string utilisateurId,
        int cohorteId,
        int challengeEtapeId,
        string? description,
        List<FichierPreuveInput> fichiersAAjouter,
        List<int>? fichierIdsARetirer)
    {
        if (!await AccesAutoriseAsync(utilisateurId))
        {
            return (false, "Accès non autorisé.", null);
        }

        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active)
        {
            return (false, "Cette Cohorte n'est pas active.", null);
        }

        var etape = await dbContext.ChallengeEtapes.FirstOrDefaultAsync(e => e.Id == challengeEtapeId);
        if (etape is null || etape.ChallengeId != cohorte.ChallengeId || etape.NumeroEtape != cohorte.EtapeCourante)
        {
            return (false, "Ce défi n'est pas (ou plus) l'étape courante de votre Cohorte.", null);
        }

        if (!await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId))
        {
            return (false, "Vous n'êtes pas membre de cette Cohorte.", null);
        }

        foreach (var fichier in fichiersAAjouter)
        {
            var erreurFichier = ValiderFichier(fichier);
            if (erreurFichier is not null)
            {
                return (false, erreurFichier, null);
            }
        }

        var preuve = await dbContext.Preuves
            .Include(p => p.Fichiers)
            .Include(p => p.ValidationsPairs)
            .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateurId && p.ChallengeEtapeId == challengeEtapeId);

        var estNouvelle = preuve is null;

        if (preuve is not null && preuve.Statut == StatutPreuve.ValideeDefinitivement)
        {
            return (false, "Cette preuve est déjà validée définitivement, elle ne peut plus être modifiée.", null);
        }

        preuve ??= new Preuve
        {
            UtilisateurId = utilisateurId,
            CohorteId = cohorteId,
            ChallengeEtapeId = challengeEtapeId,
        };

        if (fichierIdsARetirer is { Count: > 0 })
        {
            var aRetirer = preuve.Fichiers.Where(f => fichierIdsARetirer.Contains(f.Id)).ToList();
            foreach (var fichier in aRetirer)
            {
                await stockageService.SupprimerAsync(fichier.CheminStockage);
                preuve.Fichiers.Remove(fichier);
                dbContext.PreuveFichiers.Remove(fichier);
            }
        }

        foreach (var fichier in fichiersAAjouter)
        {
            var extension = Path.GetExtension(fichier.NomFichier);
            var typeFichier = ExtensionsAutorisees[extension];
            var cheminStockage = await stockageService.EnregistrerAsync(fichier.Contenu, fichier.NomFichier);

            preuve.Fichiers.Add(new PreuveFichier
            {
                TypeFichier = typeFichier,
                NomFichier = fichier.NomFichier,
                CheminStockage = cheminStockage,
                TailleOctets = fichier.TailleOctets,
            });
        }

        var descriptionNormalisee = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (preuve.Fichiers.Count == 0 && descriptionNormalisee is null)
        {
            return (false, "Une preuve doit contenir au moins un fichier ou une description.", null);
        }

        preuve.Description = descriptionNormalisee;
        preuve.DateDepot = DateTime.UtcNow;
        preuve.Statut = StatutPreuve.Soumise;

        // Toute modification invalide les validations pairs deja donnees : elles ont
        // evalue une version desormais perimee du contenu (cf. prompt section 2). Les
        // validations Gestionnaire, elles, restent (fil d'historique, section 6).
        if (!estNouvelle && preuve.ValidationsPairs.Count > 0)
        {
            dbContext.PreuveValidationsPairs.RemoveRange(preuve.ValidationsPairs);
            preuve.ValidationsPairs.Clear();
        }

        if (estNouvelle)
        {
            dbContext.Preuves.Add(preuve);
        }

        await dbContext.SaveChangesAsync();

        return (true, null, preuve.Id);
    }

    public async Task<PreuveDetailInfo?> GetMaPreuveAsync(string utilisateurId, int challengeEtapeId)
    {
        var preuve = await dbContext.Preuves
            .Include(p => p.ChallengeEtape)
            .Include(p => p.Fichiers)
            .Include(p => p.ValidationsPairs).ThenInclude(v => v.Valideur)
            .Include(p => p.ValidationsGestionnaire).ThenInclude(v => v.Valideur)
            .FirstOrDefaultAsync(p => p.UtilisateurId == utilisateurId && p.ChallengeEtapeId == challengeEtapeId);

        return preuve is null ? null : VersDetail(preuve);
    }

    public async Task<PreuveDetailInfo?> GetDetailPourGestionnaireAsync(int preuveId)
    {
        var preuve = await dbContext.Preuves
            .Include(p => p.ChallengeEtape)
            .Include(p => p.Fichiers)
            .Include(p => p.ValidationsPairs).ThenInclude(v => v.Valideur)
            .Include(p => p.ValidationsGestionnaire).ThenInclude(v => v.Valideur)
            .FirstOrDefaultAsync(p => p.Id == preuveId);

        return preuve is null ? null : VersDetail(preuve);
    }

    // ---- Validation par les pairs ----

    public async Task<List<PreuveAValiderInfo>> GetPreuvesAValiderAsync(string valideurId, int cohorteId)
    {
        if (!await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == valideurId))
        {
            return [];
        }

        var preuves = await dbContext.Preuves
            .Include(p => p.ChallengeEtape)
            .Include(p => p.Utilisateur)
            .Where(p => p.CohorteId == cohorteId
                && p.UtilisateurId != valideurId
                && p.Statut != StatutPreuve.ValideeDefinitivement
                && p.Statut != StatutPreuve.NonValideeALaCloture
                && !p.ValidationsPairs.Any(v => v.ValideurId == valideurId))
            .OrderBy(p => p.DateDepot)
            .ToListAsync();

        return preuves.Select(p => new PreuveAValiderInfo
        {
            PreuveId = p.Id,
            AuteurNomComplet = NomComplet(p.Utilisateur),
            TitreEtape = p.ChallengeEtape.TitreEtape,
            DateDepot = p.DateDepot,
        }).ToList();
    }

    public async Task<PreuveApercuPourPairInfo?> GetApercuPourPairAsync(int preuveId, string valideurId)
    {
        var preuve = await dbContext.Preuves
            .Include(p => p.ChallengeEtape)
            .Include(p => p.Utilisateur)
            .Include(p => p.Fichiers)
            .Include(p => p.ValidationsPairs)
            .FirstOrDefaultAsync(p => p.Id == preuveId);

        if (preuve is null || preuve.UtilisateurId == valideurId)
        {
            return null;
        }

        if (!await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == preuve.CohorteId && m.UtilisateurId == valideurId))
        {
            return null;
        }

        var maValidation = preuve.ValidationsPairs.FirstOrDefault(v => v.ValideurId == valideurId);

        return new PreuveApercuPourPairInfo
        {
            Id = preuve.Id,
            CohorteId = preuve.CohorteId,
            ChallengeEtapeId = preuve.ChallengeEtapeId,
            AuteurNomComplet = NomComplet(preuve.Utilisateur),
            TitreEtape = preuve.ChallengeEtape.TitreEtape,
            Description = preuve.Description,
            Fichiers = preuve.Fichiers.Select(VersFichierInfo).ToList(),
            MaDecisionPrecedente = maValidation?.Decision,
            MonCommentairePrecedent = maValidation?.Commentaire,
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> ValiderParPairAsync(
        int preuveId, string valideurId, DecisionValidationPair decision, string? commentaire, string lienSuiviPreuve)
    {
        var preuve = await dbContext.Preuves
            .Include(p => p.ValidationsPairs)
            .Include(p => p.ChallengeEtape).ThenInclude(e => e.Challenge)
            .FirstOrDefaultAsync(p => p.Id == preuveId);

        if (preuve is null)
        {
            return (false, "Preuve introuvable.");
        }

        if (preuve.UtilisateurId == valideurId)
        {
            return (false, "Vous ne pouvez pas valider votre propre preuve.");
        }

        if (!await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == preuve.CohorteId && m.UtilisateurId == valideurId))
        {
            return (false, "Vous n'êtes pas membre de cette Cohorte.");
        }

        var commentaireNormalise = string.IsNullOrWhiteSpace(commentaire) ? null : commentaire.Trim();
        if (decision == DecisionValidationPair.ARevoir && commentaireNormalise is null)
        {
            return (false, "Un commentaire est obligatoire pour un avis \"À revoir\".");
        }

        var validationExistante = preuve.ValidationsPairs.FirstOrDefault(v => v.ValideurId == valideurId);
        var premierVote = validationExistante is null;

        if (validationExistante is null)
        {
            // dbContext.Add() suffit : le fixup de relations d'EF Core rattache
            // automatiquement la nouvelle ligne a preuve.ValidationsPairs (deja chargee via
            // Include) - l'ajouter aussi explicitement dupliquerait l'entree dans cette
            // liste en memoire et faussait le calcul du ratio ci-dessous.
            validationExistante = new PreuveValidationPair { PreuveId = preuve.Id, ValideurId = valideurId };
            dbContext.PreuveValidationsPairs.Add(validationExistante);
        }

        validationExistante.Decision = decision;
        validationExistante.Commentaire = commentaireNormalise;
        validationExistante.DateValidation = DateTime.UtcNow;

        // L'effort de review est valorise pour Valide comme pour ARevoir - une seule fois
        // par pair sur cette preuve (pas a chaque modification d'avis).
        if (premierVote)
        {
            await AjouterPointsAsync(valideurId, preuve.CohorteId, TypePoints.PointsKarma,
                PointsConfig.PointsKarmaDecisionPair, MotifPoints.DecisionPairDonnee,
                ReferenceTypePoints.PreuveValidationPair, validationExistante.Id);
        }

        // Statut fige une fois finalise (ValideeDefinitivement ou NonValideeALaCloture) :
        // le vote reste enregistre (cf. ci-dessus) mais ne recalcule plus le statut.
        var statutAvant = preuve.Statut;
        if (preuve.Statut != StatutPreuve.ValideeDefinitivement && preuve.Statut != StatutPreuve.NonValideeALaCloture)
        {
            var total = preuve.ValidationsPairs.Count;
            var nombreValide = preuve.ValidationsPairs.Count(v => v.Decision == DecisionValidationPair.Valide);
            var ratio = total == 0 ? 0 : (double)nombreValide / total;

            preuve.Statut = total >= 1 && ratio >= PointsConfig.SeuilRatioValidationPairs
                ? StatutPreuve.ValideeParLesPairs
                : StatutPreuve.Soumise;
        }

        await dbContext.SaveChangesAsync();

        var messageDecision = decision == DecisionValidationPair.Valide
            ? "Un pair a validé ta preuve — viens voir son retour"
            : "Un pair te propose de revoir ta preuve — viens voir ce qu'il en pense";
        await notificationService.CreerAsync(preuve.UtilisateurId, TypeNotification.DecisionSurMaPreuve,
            ReferenceTypeNotification.Preuve, preuve.Id, messageDecision, lienSuiviPreuve);

        // "Une seule fois par passage" (cf. prompt section C) : uniquement sur le
        // FRANCHISSEMENT du seuil, jamais sur un recalcul qui reste/repasse au meme statut
        // (evite le spam si le ratio oscille autour de 50%).
        if (statutAvant != StatutPreuve.ValideeParLesPairs && preuve.Statut == StatutPreuve.ValideeParLesPairs)
        {
            await notificationService.CreerAsync(preuve.UtilisateurId, TypeNotification.PreuveValideeParLesPairs,
                ReferenceTypeNotification.Preuve, preuve.Id,
                "Ta preuve a été validée par tes pairs — encore une étape avant la validation finale !", lienSuiviPreuve);

            var auteur = await userManager.FindByIdAsync(preuve.UtilisateurId);
            if (!string.IsNullOrWhiteSpace(auteur?.Email))
            {
                var (sujet, corps) = ChallengeEmailTemplates.PreuveValideeParLesPairs(
                    preuve.ChallengeEtape.Challenge.Titre, preuve.ChallengeEtape.TitreEtape, lienSuiviPreuve);
                await emailService.EnvoyerAsync(auteur.Email, sujet, corps);
            }
        }

        return (true, null);
    }

    // ---- Gestionnaire ----

    public async Task<List<PreuveEtapeInfo>> GetPreuvesEtapeAsync(int cohorteId)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active)
        {
            return [];
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);
        if (etape is null)
        {
            return [];
        }

        var preuves = await dbContext.Preuves
            .Include(p => p.Utilisateur)
            .Include(p => p.ValidationsPairs)
            .Where(p => p.CohorteId == cohorteId && p.ChallengeEtapeId == etape.Id)
            .OrderBy(p => p.DateDepot)
            .ToListAsync();

        return preuves.Select(p => new PreuveEtapeInfo
        {
            PreuveId = p.Id,
            AuteurNomComplet = NomComplet(p.Utilisateur),
            Statut = p.Statut,
            DateDepot = p.DateDepot,
            NombreDecisionsPairs = p.ValidationsPairs.Count,
            NombreDecisionsValidePairs = p.ValidationsPairs.Count(v => v.Decision == DecisionValidationPair.Valide),
        }).ToList();
    }

    public async Task<ResumeClotureEtapeInfo> GetResumeAvantClotureAsync(int cohorteId)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return new ResumeClotureEtapeInfo();
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);
        if (etape is null)
        {
            return new ResumeClotureEtapeInfo { NumeroEtape = cohorte.EtapeCourante };
        }

        var preuves = await dbContext.Preuves
            .Where(p => p.CohorteId == cohorteId && p.ChallengeEtapeId == etape.Id)
            .ToListAsync();

        var nombreMembres = await dbContext.CohorteMembres.CountAsync(m => m.CohorteId == cohorteId);

        return new ResumeClotureEtapeInfo
        {
            NumeroEtape = cohorte.EtapeCourante,
            NombreValideesParLesPairs = preuves.Count(p => p.Statut == StatutPreuve.ValideeParLesPairs),
            NombreValideesDefinitivementDeja = preuves.Count(p => p.Statut == StatutPreuve.ValideeDefinitivement),
            NombreSoumisesRestantes = preuves.Count(p => p.Statut == StatutPreuve.Soumise),
            NombreSansPreuveDeposee = Math.Max(0, nombreMembres - preuves.Count),
        };
    }

    public async Task<(bool Success, string? ErrorMessage)> ValiderParGestionnaireAsync(
        int preuveId, string valideurId, DecisionValidationGestionnaire decision, string? commentaire, string lienSuiviPreuve)
    {
        var preuve = await dbContext.Preuves.FirstOrDefaultAsync(p => p.Id == preuveId);
        if (preuve is null)
        {
            return (false, "Preuve introuvable.");
        }

        if (preuve.Statut == StatutPreuve.ValideeDefinitivement)
        {
            return (false, "Cette preuve est déjà validée définitivement.");
        }

        var commentaireNormalise = string.IsNullOrWhiteSpace(commentaire) ? null : commentaire.Trim();
        if (decision == DecisionValidationGestionnaire.Refuse && commentaireNormalise is null)
        {
            return (false, "Un commentaire est obligatoire en cas de refus.");
        }

        var validation = new PreuveValidationGestionnaire
        {
            PreuveId = preuve.Id,
            ValideurId = valideurId,
            Decision = decision,
            Commentaire = commentaireNormalise,
            DateValidation = DateTime.UtcNow,
        };
        dbContext.PreuveValidationsGestionnaire.Add(validation);

        if (decision == DecisionValidationGestionnaire.Valide)
        {
            preuve.Statut = StatutPreuve.ValideeDefinitivement;
            await dbContext.SaveChangesAsync();

            await AjouterPointsAsync(preuve.UtilisateurId, preuve.CohorteId, TypePoints.XPSavoir,
                PointsConfig.XPSavoirPreuveValidee, MotifPoints.PreuveValideeDefinitivement,
                ReferenceTypePoints.PreuveValidationGestionnaire, validation.Id);
        }
        else
        {
            preuve.Statut = StatutPreuve.Soumise;
            await dbContext.SaveChangesAsync();
        }

        var messageDecision = decision == DecisionValidationGestionnaire.Valide
            ? "Ton Coach a validé ta preuve — bravo !"
            : "Ton Coach te propose de revoir ta preuve — viens voir ce qu'il en pense";
        await notificationService.CreerAsync(preuve.UtilisateurId, TypeNotification.DecisionSurMaPreuve,
            ReferenceTypeNotification.Preuve, preuve.Id, messageDecision, lienSuiviPreuve);

        return (true, null);
    }

    public async Task ClorePreuvesEtapeAsync(int cohorteId, int numeroEtape)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return;
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == numeroEtape);
        if (etape is null)
        {
            return;
        }

        var preuves = await dbContext.Preuves
            .Where(p => p.CohorteId == cohorteId && p.ChallengeEtapeId == etape.Id)
            .ToListAsync();

        foreach (var preuve in preuves)
        {
            if (preuve.Statut == StatutPreuve.ValideeParLesPairs)
            {
                preuve.Statut = StatutPreuve.ValideeDefinitivement;
                await AjouterPointsAsync(preuve.UtilisateurId, cohorteId, TypePoints.XPSavoir,
                    PointsConfig.XPSavoirPreuveValidee, MotifPoints.PreuveValideeDefinitivement,
                    ReferenceTypePoints.Preuve, preuve.Id);
                await AjouterPointsAsync(preuve.UtilisateurId, cohorteId, TypePoints.PointsAssiduite,
                    PointsConfig.PointsAssiduitePreuveValideeALaCloture, MotifPoints.PreuveValideeParLesPairsALaCloture,
                    ReferenceTypePoints.Preuve, preuve.Id);
            }
            else if (preuve.Statut == StatutPreuve.Soumise)
            {
                preuve.Statut = StatutPreuve.NonValideeALaCloture;
            }
            // ValideeDefinitivement (deja acquise via 4.1) : on ne touche a rien, pas de
            // double comptage.
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task AttribuerBadgeSuperHelperAsync(int cohorteId, int numeroEtapeCloturee)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return;
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == numeroEtapeCloturee);
        if (etape is null)
        {
            return;
        }

        var depuis = await dbContext.CohorteEtapeValidations
            .Where(v => v.CohorteId == cohorteId && v.NumeroEtape == numeroEtapeCloturee - 1)
            .Select(v => (DateTime?)v.ValideLe)
            .FirstOrDefaultAsync() ?? DateTime.MinValue;

        var totauxKarma = await dbContext.PointsEvenements
            .Where(e => e.CohorteId == cohorteId && e.TypePoints == TypePoints.PointsKarma && e.Date > depuis)
            .GroupBy(e => e.UtilisateurId)
            .Select(g => new { UtilisateurId = g.Key, Total = g.Sum(e => e.Montant) })
            .ToListAsync();

        if (totauxKarma.Count == 0)
        {
            return;
        }

        var maximum = totauxKarma.Max(t => t.Total);
        if (maximum <= 0)
        {
            return;
        }

        var gagnants = totauxKarma.Where(t => t.Total == maximum).Select(t => t.UtilisateurId).ToList();

        var dejaAttribues = await dbContext.BadgeSocialAttributions
            .Where(b => b.CohorteId == cohorteId && b.ChallengeEtapeId == etape.Id && b.TypeBadge == TypeBadgeSocial.SuperHelper)
            .Select(b => b.UtilisateurId)
            .ToListAsync();

        foreach (var utilisateurId in gagnants.Except(dejaAttribues))
        {
            dbContext.BadgeSocialAttributions.Add(new BadgeSocialAttribution
            {
                UtilisateurId = utilisateurId,
                CohorteId = cohorteId,
                ChallengeEtapeId = etape.Id,
                TypeBadge = TypeBadgeSocial.SuperHelper,
            });
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<List<ContributionMembreInfo>> GetContributionsCohorteAsync(int cohorteId)
    {
        var membres = await dbContext.CohorteMembres
            .Include(m => m.Utilisateur)
            .Where(m => m.CohorteId == cohorteId)
            .ToListAsync();

        var evenements = await dbContext.PointsEvenements
            .Where(e => e.CohorteId == cohorteId)
            .ToListAsync();

        var preuvesCohorte = await dbContext.Preuves
            .Where(p => p.CohorteId == cohorteId)
            .Select(p => new { p.Id, p.UtilisateurId })
            .ToListAsync();

        var validationsParPair = await dbContext.PreuveValidationsPairs
            .Where(v => v.Preuve.CohorteId == cohorteId)
            .Select(v => new { v.ValideurId, v.PreuveId })
            .ToListAsync();

        return membres.Select(membre =>
        {
            var evenementsMembre = evenements.Where(e => e.UtilisateurId == membre.UtilisateurId).ToList();
            var preuvesAttendues = preuvesCohorte.Count(p => p.UtilisateurId != membre.UtilisateurId);
            var preuvesEvaluees = validationsParPair.Count(v => v.ValideurId == membre.UtilisateurId);

            return new ContributionMembreInfo
            {
                UtilisateurId = membre.UtilisateurId,
                NomComplet = NomComplet(membre.Utilisateur),
                XPSavoir = evenementsMembre.Where(e => e.TypePoints == TypePoints.XPSavoir).Sum(e => e.Montant),
                PointsKarma = evenementsMembre.Where(e => e.TypePoints == TypePoints.PointsKarma).Sum(e => e.Montant),
                PointsAssiduite = evenementsMembre.Where(e => e.TypePoints == TypePoints.PointsAssiduite).Sum(e => e.Montant),
                PreuvesEvalueesParCeMembre = preuvesEvaluees,
                PreuvesAttenduesPourCeMembre = preuvesAttendues,
            };
        }).ToList();
    }

    // ---- Points et badges (apprenant) ----

    public async Task<PointsResumeInfo> GetMesPointsAsync(string utilisateurId)
    {
        var evenements = await dbContext.PointsEvenements
            .Where(e => e.UtilisateurId == utilisateurId)
            .ToListAsync();

        var etapesParEvenement = await ResoudreEtapesAsync(evenements);

        var detailParEtape = evenements
            .Select(e => (Evenement: e, Etape: etapesParEvenement.GetValueOrDefault(e.Id)))
            .Where(x => x.Etape is not null)
            .GroupBy(x => x.Etape!.Value)
            .Select(g => new PointsParEtapeInfo
            {
                NumeroEtape = g.Key.NumeroEtape,
                TitreEtape = g.Key.TitreEtape,
                XPSavoir = g.Where(x => x.Evenement.TypePoints == TypePoints.XPSavoir).Sum(x => x.Evenement.Montant),
                PointsKarma = g.Where(x => x.Evenement.TypePoints == TypePoints.PointsKarma).Sum(x => x.Evenement.Montant),
                PointsAssiduite = g.Where(x => x.Evenement.TypePoints == TypePoints.PointsAssiduite).Sum(x => x.Evenement.Montant),
            })
            .OrderBy(d => d.NumeroEtape)
            .ToList();

        return new PointsResumeInfo
        {
            TotalXPSavoir = evenements.Where(e => e.TypePoints == TypePoints.XPSavoir).Sum(e => e.Montant),
            TotalPointsKarma = evenements.Where(e => e.TypePoints == TypePoints.PointsKarma).Sum(e => e.Montant),
            TotalPointsAssiduite = evenements.Where(e => e.TypePoints == TypePoints.PointsAssiduite).Sum(e => e.Montant),
            DetailParEtape = detailParEtape,
        };
    }

    public async Task<List<PointsEvenementInfo>> GetMonHistoriquePointsAsync(string utilisateurId)
    {
        var evenements = await dbContext.PointsEvenements
            .Where(e => e.UtilisateurId == utilisateurId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        var etapesParEvenement = await ResoudreEtapesAsync(evenements);

        return evenements.Select(e => new PointsEvenementInfo
        {
            Date = e.Date,
            TypePoints = e.TypePoints,
            Montant = e.Montant,
            Motif = e.Motif,
            NumeroEtape = etapesParEvenement.GetValueOrDefault(e.Id)?.NumeroEtape,
        }).ToList();
    }

    public async Task<List<BadgeSocialInfo>> GetMesBadgesAsync(string utilisateurId)
    {
        var badges = await dbContext.BadgeSocialAttributions
            .Include(b => b.ChallengeEtape).ThenInclude(e => e.Challenge)
            .Where(b => b.UtilisateurId == utilisateurId)
            .OrderByDescending(b => b.DateAttribution)
            .ToListAsync();

        return badges.Select(b => new BadgeSocialInfo
        {
            TypeBadge = b.TypeBadge,
            NumeroEtape = b.ChallengeEtape.NumeroEtape,
            TitreEtape = b.ChallengeEtape.TitreEtape,
            ChallengeTitre = b.ChallengeEtape.Challenge.Titre,
            DateAttribution = b.DateAttribution,
        }).ToList();
    }

    // ---- Fichiers ----

    public async Task<(Stream Contenu, string NomFichier)?> TelechargerFichierAsync(int fichierId, string utilisateurId, bool aLeDroitAdmin)
    {
        var fichier = await dbContext.PreuveFichiers
            .Include(f => f.Preuve)
            .FirstOrDefaultAsync(f => f.Id == fichierId);

        if (fichier is null)
        {
            return null;
        }

        // aLeDroitAdmin couvre PREUVE.CONSULTER comme PREUVE.VALIDER (calcule par
        // l'appelant) : un Gestionnaire/Coach/Chef de Projet en lecture seule doit pouvoir
        // consulter les fichiers deposes, pas seulement celui qui peut valider (correction
        // A.1 - avant cette correction, seul PREUVE.VALIDER donnait acces).
        var estAutorise = aLeDroitAdmin
            || fichier.Preuve.UtilisateurId == utilisateurId
            || await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == fichier.Preuve.CohorteId && m.UtilisateurId == utilisateurId);

        if (!estAutorise)
        {
            return null;
        }

        var flux = await stockageService.TelechargerAsync(fichier.CheminStockage);
        return flux is null ? null : (flux, fichier.NomFichier);
    }

    // ---- Helpers ----

    private async Task<bool> AccesAutoriseAsync(string utilisateurId)
    {
        var utilisateur = await userManager.FindByIdAsync(utilisateurId);
        return utilisateur is not null && (utilisateur.EstSuperAdministrateur || utilisateur.Statut == StatutUtilisateur.Actif);
    }

    private static string? ValiderFichier(FichierPreuveInput fichier)
    {
        var extension = Path.GetExtension(fichier.NomFichier);
        if (!ExtensionsAutorisees.ContainsKey(extension))
        {
            return $"Format non accepté pour \"{fichier.NomFichier}\" (formats acceptés : doc, docx, pdf, jpg, jpeg, png, webp, mp4, mov).";
        }

        if (fichier.TailleOctets > TailleMaxFichierOctets)
        {
            return $"\"{fichier.NomFichier}\" dépasse la taille maximale autorisée (50 Mo).";
        }

        return null;
    }

    private async Task AjouterPointsAsync(
        string utilisateurId, int? cohorteId, TypePoints typePoints, int montant,
        MotifPoints motif, ReferenceTypePoints referenceType, int referenceId)
    {
        dbContext.PointsEvenements.Add(new PointsEvenement
        {
            UtilisateurId = utilisateurId,
            CohorteId = cohorteId,
            TypePoints = typePoints,
            Montant = montant,
            Motif = motif,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Date = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();
    }

    // Resout, pour chaque PointsEvenement, l'etape (numero + titre) a laquelle il se
    // rattache en remontant sa reference polymorphe (Preuve directe, ou via
    // PreuveValidationPair/PreuveValidationGestionnaire/ForumMessageUtile). PointsEvenement
    // ne porte pas directement de ChallengeEtapeId (cf. schema du prompt) - cette
    // resolution est necessaire pour le detail "par etape" de la vue Mes points.
    private async Task<Dictionary<int, (int NumeroEtape, string TitreEtape)?>> ResoudreEtapesAsync(List<PointsEvenement> evenements)
    {
        var resultat = new Dictionary<int, (int NumeroEtape, string TitreEtape)?>();
        if (evenements.Count == 0)
        {
            return resultat;
        }

        var idsPreuve = evenements.Where(e => e.ReferenceType == ReferenceTypePoints.Preuve).Select(e => e.ReferenceId).Distinct().ToList();
        var idsValidationPair = evenements.Where(e => e.ReferenceType == ReferenceTypePoints.PreuveValidationPair).Select(e => e.ReferenceId).Distinct().ToList();
        var idsValidationGestionnaire = evenements.Where(e => e.ReferenceType == ReferenceTypePoints.PreuveValidationGestionnaire).Select(e => e.ReferenceId).Distinct().ToList();
        var idsMessageUtile = evenements.Where(e => e.ReferenceType == ReferenceTypePoints.ForumMessageUtile).Select(e => e.ReferenceId).Distinct().ToList();

        var etapesParPreuveId = await dbContext.Preuves
            .Where(p => idsPreuve.Contains(p.Id))
            .Select(p => new { p.Id, p.ChallengeEtape.NumeroEtape, p.ChallengeEtape.TitreEtape })
            .ToDictionaryAsync(p => p.Id, p => (p.NumeroEtape, p.TitreEtape));

        var etapesParValidationPairId = await dbContext.PreuveValidationsPairs
            .Where(v => idsValidationPair.Contains(v.Id))
            .Select(v => new { v.Id, v.Preuve.ChallengeEtape.NumeroEtape, v.Preuve.ChallengeEtape.TitreEtape })
            .ToDictionaryAsync(v => v.Id, v => (v.NumeroEtape, v.TitreEtape));

        var etapesParValidationGestionnaireId = await dbContext.PreuveValidationsGestionnaire
            .Where(v => idsValidationGestionnaire.Contains(v.Id))
            .Select(v => new { v.Id, v.Preuve.ChallengeEtape.NumeroEtape, v.Preuve.ChallengeEtape.TitreEtape })
            .ToDictionaryAsync(v => v.Id, v => (v.NumeroEtape, v.TitreEtape));

        var etapesParMessageUtileId = await dbContext.ForumMessagesUtiles
            .Where(u => idsMessageUtile.Contains(u.Id))
            .Select(u => new { u.Id, u.Message.ChallengeEtape.NumeroEtape, u.Message.ChallengeEtape.TitreEtape })
            .ToDictionaryAsync(u => u.Id, u => (u.NumeroEtape, u.TitreEtape));

        foreach (var evenement in evenements)
        {
            resultat[evenement.Id] = evenement.ReferenceType switch
            {
                ReferenceTypePoints.Preuve when etapesParPreuveId.TryGetValue(evenement.ReferenceId, out var e) => e,
                ReferenceTypePoints.PreuveValidationPair when etapesParValidationPairId.TryGetValue(evenement.ReferenceId, out var e) => e,
                ReferenceTypePoints.PreuveValidationGestionnaire when etapesParValidationGestionnaireId.TryGetValue(evenement.ReferenceId, out var e) => e,
                ReferenceTypePoints.ForumMessageUtile when etapesParMessageUtileId.TryGetValue(evenement.ReferenceId, out var e) => e,
                _ => null,
            };
        }

        return resultat;
    }

    private static PreuveFichierInfo VersFichierInfo(PreuveFichier f) => new()
    {
        Id = f.Id,
        TypeFichier = f.TypeFichier,
        NomFichier = f.NomFichier,
        TailleOctets = f.TailleOctets,
    };

    private static PreuveDetailInfo VersDetail(Preuve p)
    {
        var retours = new List<ValidationRecue>();

        retours.AddRange(p.ValidationsPairs.Select(v => new ValidationRecue
        {
            ValideurNomComplet = NomComplet(v.Valideur),
            EstGestionnaire = false,
            Decision = v.Decision == DecisionValidationPair.Valide ? "Validé" : "À revoir",
            Commentaire = v.Commentaire,
            Date = v.DateValidation,
        }));

        retours.AddRange(p.ValidationsGestionnaire.Select(v => new ValidationRecue
        {
            ValideurNomComplet = NomComplet(v.Valideur),
            EstGestionnaire = true,
            Decision = v.Decision == DecisionValidationGestionnaire.Valide ? "Validé" : "Refusé",
            Commentaire = v.Commentaire,
            Date = v.DateValidation,
        }));

        return new PreuveDetailInfo
        {
            Id = p.Id,
            ChallengeEtapeId = p.ChallengeEtapeId,
            TitreEtape = p.ChallengeEtape.TitreEtape,
            Description = p.Description,
            Statut = p.Statut,
            DateDepot = p.DateDepot,
            Fichiers = p.Fichiers.Select(VersFichierInfo).ToList(),
            Retours = retours.OrderBy(r => r.Date).ToList(),
        };
    }

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}
