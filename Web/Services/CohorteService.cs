using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class CohorteService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    IPreuveService preuveService,
    INotificationService notificationService) : ICohorteService
{
    public async Task<List<CohorteResume>> GetAllAsync()
    {
        // Les Cohortes Proposee ne sont pas de "vraies" Cohortes tant qu'un Gestionnaire ne
        // les a pas validees (cf. GetDemandesEmbarquementAsync/ValiderEmbarquementAsync,
        // ecran dedie "Demandes d'embarquement") : exclues de la liste generale pour ne pas
        // se retrouver affichees avec un statut ambigu.
        var cohortes = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Organisation)
            .Include(c => c.Membres)
            .Where(c => c.Statut != StatutCohorte.Proposee)
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync();

        var resumes = cohortes.Select(VersResume).ToList();
        await RemplirChallengeEtapeCouranteIdAsync(cohortes, resumes);
        return resumes;
    }

    public async Task<CohorteResume?> GetResumeAsync(int id)
    {
        var cohorte = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Organisation)
            .Include(c => c.Membres)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cohorte is null)
        {
            return null;
        }

        var resume = VersResume(cohorte);
        await RemplirChallengeEtapeCouranteIdAsync([cohorte], [resume]);
        return resume;
    }

    // ChallengeEtapeCouranteId n'est pas une simple projection de champs deja charges (il
    // faut resoudre (ChallengeId, EtapeCourante) -> ChallengeEtapeId) : traite en lot apres
    // VersResume plutot que dans VersResume lui-meme pour eviter une requete par Cohorte
    // (VersResume reste une simple projection synchrone, reutilisable partout sans dbContext).
    private async Task RemplirChallengeEtapeCouranteIdAsync(List<Cohorte> cohortes, List<CohorteResume> resumes)
    {
        var challengeIds = cohortes.Select(c => c.ChallengeId).Distinct().ToList();
        if (challengeIds.Count == 0)
        {
            return;
        }

        var etapes = await dbContext.ChallengeEtapes
            .Where(e => challengeIds.Contains(e.ChallengeId))
            .Select(e => new { e.Id, e.ChallengeId, e.NumeroEtape })
            .ToListAsync();

        for (var i = 0; i < cohortes.Count; i++)
        {
            var cohorte = cohortes[i];
            resumes[i].ChallengeEtapeCouranteId = etapes
                .FirstOrDefault(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante)?.Id;
        }
    }

    public async Task<List<CohorteMembreInfo>> GetMembresAsync(int id)
    {
        var membres = await dbContext.CohorteMembres
            .Include(m => m.Utilisateur)
            .Where(m => m.CohorteId == id)
            .OrderByDescending(m => m.DateAjout)
            .ToListAsync();

        return membres.Select(m => new CohorteMembreInfo
        {
            Id = m.Id,
            UtilisateurId = m.UtilisateurId,
            NomComplet = NomComplet(m.Utilisateur),
            Email = m.Utilisateur.Email ?? "-",
            MethodeAjout = m.MethodeAjout,
            DateAjout = m.DateAjout,
            StatutAcces = m.Utilisateur.Statut,
        }).ToList();
    }

    public async Task<List<CohorteEtapeValidationInfo>> GetHistoriqueValidationsAsync(int id)
    {
        var validations = await dbContext.CohorteEtapeValidations
            .Include(v => v.ValidePar)
            .Where(v => v.CohorteId == id)
            .OrderBy(v => v.NumeroEtape)
            .ToListAsync();

        return validations.Select(v => new CohorteEtapeValidationInfo
        {
            NumeroEtape = v.NumeroEtape,
            ValideParNomComplet = NomComplet(v.ValidePar),
            ValideLe = v.ValideLe,
        }).ToList();
    }

    public async Task<(bool Success, string? ErrorMessage, int? CohorteId)> CreateAsync(CohorteInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Nom))
        {
            return (false, "Le nom de la Cohorte est obligatoire.", null);
        }

        var challenge = await dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == input.ChallengeId);
        if (challenge is null)
        {
            return (false, "Challenge introuvable.", null);
        }

        if (challenge.Statut != StatutChallenge.Publie)
        {
            return (false, "Impossible de créer une Cohorte à partir d'un Challenge en Brouillon : publiez-le d'abord.", null);
        }

        if (challenge.Mode == ModePlateforme.BtoB && input.OrganisationId is null)
        {
            return (false, "Une Cohorte issue d'un Challenge BtoB doit être rattachée à une entreprise.", null);
        }

        var cohorte = new Cohorte
        {
            ChallengeId = input.ChallengeId,
            Nom = input.Nom.Trim(),
            DateLancement = input.DateLancement,
            OrganisationId = challenge.Mode == ModePlateforme.BtoB ? input.OrganisationId : null,
            Statut = StatutCohorte.EnPreparation,
            EtapeCourante = 0,
        };

        dbContext.Cohortes.Add(cohorte);
        await dbContext.SaveChangesAsync();

        return (true, null, cohorte.Id);
    }

    public async Task<(bool Success, string? ErrorMessage)> AjouterMembreManuelAsync(int cohorteId, string utilisateurId)
    {
        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        var erreur = VerifierInscriptionPossible(cohorte);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        var utilisateur = await userManager.FindByIdAsync(utilisateurId);
        if (utilisateur is null)
        {
            return (false, "Utilisateur introuvable.");
        }

        if (await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId))
        {
            return (false, "Cet utilisateur est déjà membre de cette Cohorte.");
        }

        dbContext.CohorteMembres.Add(new CohorteMembre
        {
            CohorteId = cohorteId,
            UtilisateurId = utilisateurId,
            MethodeAjout = MethodeAjoutMembre.Manuel,
            DateAjout = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        await RattraperCartesMembreAsync(cohorte, utilisateurId, utilisateurId);

        return (true, null);
    }

    public async Task<ImportMembresRapport> ImporterMembresAsync(
        int cohorteId,
        List<MembreImportInput> membres,
        string gestionnaireId,
        Func<string, string> construireLienActivation)
    {
        var rapport = new ImportMembresRapport();

        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            rapport.Erreurs.Add("Cohorte introuvable.");
            return rapport;
        }

        var erreurInscription = VerifierInscriptionPossible(cohorte);
        if (erreurInscription is not null)
        {
            rapport.Erreurs.Add(erreurInscription);
            return rapport;
        }

        foreach (var membre in membres)
        {
            var email = membre.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                rapport.Erreurs.Add("Ligne ignorée : email manquant.");
                continue;
            }

            try
            {
                var utilisateur = await userManager.FindByEmailAsync(email);

                if (utilisateur is null)
                {
                    utilisateur = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        Prenom = membre.Prenom,
                        Nom = membre.Nom,
                        Mode = ModePlateforme.BtoB,
                        // Confirme d'office : c'est le Gestionnaire qui vouche pour cet
                        // email en l'important, pas d'auto-confirmation a faire faire par
                        // l'utilisateur (il n'a pas encore de mot de passe pour se
                        // connecter de toute facon).
                        EmailConfirmed = true,
                    };

                    var resultat = await userManager.CreateAsync(utilisateur);
                    if (!resultat.Succeeded)
                    {
                        rapport.Erreurs.Add($"{email} : {string.Join("; ", resultat.Errors.Select(e => e.Description))}");
                        continue;
                    }

                    await CreerEtEnvoyerInvitationAsync(utilisateur, construireLienActivation);
                    rapport.ComptesCrees++;
                }
                else
                {
                    rapport.ComptesExistantsRattaches++;
                }

                if (await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateur.Id))
                {
                    rapport.DejaMembres++;
                    continue;
                }

                dbContext.CohorteMembres.Add(new CohorteMembre
                {
                    CohorteId = cohorteId,
                    UtilisateurId = utilisateur.Id,
                    MethodeAjout = MethodeAjoutMembre.Import,
                    DateAjout = DateTime.UtcNow,
                });
                await dbContext.SaveChangesAsync();

                await RattraperCartesMembreAsync(cohorte, utilisateur.Id, gestionnaireId);
            }
            catch (Exception ex)
            {
                rapport.Erreurs.Add($"{email} : {ex.Message}");
            }
        }

        return rapport;
    }

    public async Task<(bool Success, string? ErrorMessage)> AutoInscrireAsync(int cohorteId, string utilisateurId)
    {
        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Challenge.Mode != ModePlateforme.BtoC)
        {
            return (false, "L'auto-inscription n'est disponible que pour les Challenges BtoC.");
        }

        var erreur = VerifierInscriptionPossible(cohorte);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        if (await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteId && m.UtilisateurId == utilisateurId))
        {
            return (true, null);
        }

        dbContext.CohorteMembres.Add(new CohorteMembre
        {
            CohorteId = cohorteId,
            UtilisateurId = utilisateurId,
            MethodeAjout = MethodeAjoutMembre.AutoInscription,
            DateAjout = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RenvoyerInvitationAsync(string utilisateurId, Func<string, string> construireLienActivation)
    {
        var utilisateur = await userManager.FindByIdAsync(utilisateurId);
        if (utilisateur is null)
        {
            return (false, "Utilisateur introuvable.");
        }

        var anciennesInvitations = await dbContext.InvitationsComptes
            .Where(i => i.UtilisateurId == utilisateurId && i.EstActif)
            .ToListAsync();

        foreach (var ancienne in anciennesInvitations)
        {
            ancienne.EstActif = false;
        }

        await CreerEtEnvoyerInvitationAsync(utilisateur, construireLienActivation);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> LancerAsync(int cohorteId, string gestionnaireId, string lienMonParcours, string lienQuestionnaireMiParcours)
    {
        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.EnPreparation)
        {
            return (false, "Seule une Cohorte en préparation peut être lancée.");
        }

        cohorte.Statut = StatutCohorte.Active;
        cohorte.EtapeCourante = 1;
        await dbContext.SaveChangesAsync();

        await AttribuerCartesEtapeAsync(cohorte, 1, gestionnaireId);
        await NotifierLancementAsync(cohorte, lienMonParcours);
        await EnvoyerQuestionnaireMiParcoursSiEtapeMedianeAsync(cohorte, 1, lienQuestionnaireMiParcours);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ValiderEtapeAsync(
        int cohorteId,
        string gestionnaireId,
        string lienMonParcours,
        string lienBibliotheque,
        string lienSatisfaction,
        string lienQuestionnaireMiParcours,
        string lienAttestation)
    {
        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstOrDefaultAsync(c => c.Id == cohorteId);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.Active)
        {
            return (false, "Seule une Cohorte active peut être avancée.");
        }

        // Suivi de l'execution Qualiopi : impossible d'avancer tant que tous les emargements
        // de l'etape en cours ne sont pas signes - force a utiliser le circuit emargement
        // (cf. EmargementService) plutot que de le laisser optionnel. Ne bloque rien si
        // l'etape n'a aucune carte attribuee (rien a emarger dans ce cas).
        if (!await TousLesEmargementsSontSignesAsync(cohorteId, cohorte.ChallengeId, cohorte.EtapeCourante))
        {
            return (false, "Impossible de valider cette étape : tous les membres n'ont pas encore signé leur émargement.");
        }

        // Suivi des acquis Qualiopi (Methode Miroir) : le test de connaissances en amont
        // doit etre complet avant de quitter l'etape 1, celui en aval avant de cloturer -
        // cf. TestPositionnementService, jamais de branche automatique sans envoi explicite
        // du Gestionnaire au prealable (GetTestRequisManquantAsync renvoie false tant que la
        // campagne n'a meme pas ete envoyee).
        var testManquant = await GetTestRequisManquantAsync(cohorte);
        if (testManquant is not null)
        {
            var libelle = testManquant == TypeTestPositionnement.Amont ? "en amont" : "en aval";
            return (false, $"Impossible de valider cette étape : tous les membres n'ont pas encore répondu au test de connaissances {libelle}.");
        }

        var etapeValidee = cohorte.EtapeCourante;

        dbContext.CohorteEtapeValidations.Add(new CohorteEtapeValidation
        {
            CohorteId = cohorteId,
            NumeroEtape = etapeValidee,
            ValideParId = gestionnaireId,
            ValideLe = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        // Extension "Depot de preuves, points et forum par etape" : finalise en bloc les
        // Preuves de l'etape qui se cloture (section 4.2), puis calcule le badge social
        // Super Helper de la periode (section 5) - toujours declenche par cette meme
        // action humaine explicite, jamais par une tache planifiee (cf. prompt section 8).
        await preuveService.ClorePreuvesEtapeAsync(cohorteId, etapeValidee);
        await preuveService.AttribuerBadgeSuperHelperAsync(cohorteId, etapeValidee);

        if (cohorte.EtapeCourante >= cohorte.Challenge.NombreEtapes)
        {
            cohorte.Statut = StatutCohorte.Terminee;
            await dbContext.SaveChangesAsync();

            await NotifierClotureAsync(cohorte, lienBibliotheque, lienAttestation);
            await NotifierDemandeSatisfactionAsync(cohorte, lienSatisfaction);
            return (true, null);
        }

        cohorte.EtapeCourante++;
        await dbContext.SaveChangesAsync();

        await AttribuerCartesEtapeAsync(cohorte, cohorte.EtapeCourante, gestionnaireId);
        await NotifierNouvelleEtapeAsync(cohorte, cohorte.EtapeCourante, lienMonParcours);
        await EnvoyerQuestionnaireMiParcoursSiEtapeMedianeAsync(cohorte, cohorte.EtapeCourante, lienQuestionnaireMiParcours);

        return (true, null);
    }

    public async Task<List<ParcoursEnCoursInfo>> GetMesParcoursEnCoursAsync(string utilisateurId)
    {
        var utilisateur = await userManager.FindByIdAsync(utilisateurId);
        if (utilisateur is null || (!utilisateur.EstSuperAdministrateur && utilisateur.Statut != StatutUtilisateur.Actif))
        {
            return [];
        }

        var membres = await dbContext.CohorteMembres
            .Where(m => m.UtilisateurId == utilisateurId && m.Cohorte.Statut == StatutCohorte.Active)
            .Include(m => m.Cohorte)
                .ThenInclude(c => c.Challenge)
            .ToListAsync();

        var resultat = new List<ParcoursEnCoursInfo>();
        foreach (var membre in membres)
        {
            var cohorte = membre.Cohorte;

            var etape = await dbContext.ChallengeEtapes
                .Include(e => e.Cartes)
                    .ThenInclude(ec => ec.CarteCompetence)
                        .ThenInclude(c => c.Badge)
                .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == cohorte.EtapeCourante);

            if (etape is null)
            {
                continue;
            }

            // Les cartes et instructions personnalisees n'existent que sur les parcours Bilan
            // de competences individuel (cf. CohorteMembreCarteSupplementaire,
            // CohorteMembreEtapePersonnalisation) - inutile d'interroger ces tables sur un
            // Challenge Collectif.
            var defiIndividuel = etape.DefiIndividuel;
            List<CarteCompetence> cartesPersonnalisees = [];
            if (cohorte.Challenge.Format == FormatChallenge.BilanCompetencesIndividuel)
            {
                cartesPersonnalisees = await dbContext.CohorteMembreCartesSupplementaires
                    .Where(cs => cs.CohorteMembreId == membre.Id && cs.ChallengeEtapeId == etape.Id)
                    .Include(cs => cs.CarteCompetence)
                        .ThenInclude(c => c.Badge)
                    .Select(cs => cs.CarteCompetence)
                    .ToListAsync();

                // Remplacement pour ce membre uniquement (jamais de modification du template
                // partage ChallengeEtape.DefiIndividuel) - cf. CohorteMembreEtapePersonnalisation.
                var instructionsPersonnalisees = await dbContext.CohorteMembreEtapePersonnalisations
                    .Where(p => p.CohorteMembreId == membre.Id && p.ChallengeEtapeId == etape.Id)
                    .Select(p => p.DefiIndividuelPersonnalise)
                    .FirstOrDefaultAsync();
                if (instructionsPersonnalisees is not null)
                {
                    defiIndividuel = instructionsPersonnalisees;
                }
            }

            resultat.Add(new ParcoursEnCoursInfo
            {
                CohorteId = cohorte.Id,
                ChallengeTitre = cohorte.Challenge.Titre,
                ChallengeEtapeId = etape.Id,
                NumeroEtape = etape.NumeroEtape,
                TitreEtape = etape.TitreEtape,
                DefiIndividuel = defiIndividuel,
                Cartes = etape.Cartes.Select(ec => ec.CarteCompetence).ToList(),
                CartesPersonnalisees = cartesPersonnalisees,
            });
        }

        return resultat;
    }

    // Liste TOUTES les etapes du Challenge (pas seulement l'etape courante de la Cohorte) :
    // permet au Coach de naviguer vers une etape future pour la preparer en avance, pas
    // seulement l'etape en cours.
    public async Task<List<PersonnalisationEtapeInfo>?> GetEtapesPersonnalisationAsync(int cohorteId, int cohorteMembreId)
    {
        var membre = await dbContext.CohorteMembres
            .Include(m => m.Cohorte)
            .FirstOrDefaultAsync(m => m.Id == cohorteMembreId && m.CohorteId == cohorteId);

        if (membre is null)
        {
            return null;
        }

        var etapes = await dbContext.ChallengeEtapes
            .Where(e => e.ChallengeId == membre.Cohorte.ChallengeId)
            .OrderBy(e => e.NumeroEtape)
            .ToListAsync();

        var nombreCartesParEtape = await dbContext.CohorteMembreCartesSupplementaires
            .Where(cs => cs.CohorteMembreId == cohorteMembreId)
            .GroupBy(cs => cs.ChallengeEtapeId)
            .Select(g => new { ChallengeEtapeId = g.Key, Nombre = g.Count() })
            .ToListAsync();

        var etapesAvecInstructions = await dbContext.CohorteMembreEtapePersonnalisations
            .Where(p => p.CohorteMembreId == cohorteMembreId)
            .Select(p => p.ChallengeEtapeId)
            .ToListAsync();

        return etapes.Select(e => new PersonnalisationEtapeInfo
        {
            ChallengeEtapeId = e.Id,
            NumeroEtape = e.NumeroEtape,
            TitreEtape = e.TitreEtape,
            EstEtapeCourante = e.NumeroEtape == membre.Cohorte.EtapeCourante,
            NombreCartesPersonnalisees = nombreCartesParEtape.FirstOrDefault(x => x.ChallengeEtapeId == e.Id)?.Nombre ?? 0,
            AInstructionsPersonnalisees = etapesAvecInstructions.Contains(e.Id),
        }).ToList();
    }

    public async Task<PersonnalisationCartesContexte?> GetContextePersonnalisationCartesAsync(int cohorteId, int cohorteMembreId, int challengeEtapeId)
    {
        var membre = await dbContext.CohorteMembres
            .Include(m => m.Utilisateur)
            .Include(m => m.Cohorte)
                .ThenInclude(c => c.Challenge)
            .FirstOrDefaultAsync(m => m.Id == cohorteMembreId && m.CohorteId == cohorteId);

        if (membre is null)
        {
            return null;
        }

        var etape = await dbContext.ChallengeEtapes
            .Include(e => e.Cartes)
            .FirstOrDefaultAsync(e => e.Id == challengeEtapeId && e.ChallengeId == membre.Cohorte.ChallengeId);

        if (etape is null)
        {
            return null;
        }

        var supplementaires = await dbContext.CohorteMembreCartesSupplementaires
            .Include(cs => cs.CarteCompetence)
            .Include(cs => cs.AjouteePar)
            .Where(cs => cs.CohorteMembreId == cohorteMembreId && cs.ChallengeEtapeId == challengeEtapeId)
            .OrderBy(cs => cs.AjouteeLe)
            .ToListAsync();

        var instructionsPersonnalisees = await dbContext.CohorteMembreEtapePersonnalisations
            .Where(p => p.CohorteMembreId == cohorteMembreId && p.ChallengeEtapeId == challengeEtapeId)
            .Select(p => p.DefiIndividuelPersonnalise)
            .FirstOrDefaultAsync();

        return new PersonnalisationCartesContexte
        {
            CohorteId = cohorteId,
            CohorteMembreId = cohorteMembreId,
            MembreNomComplet = NomComplet(membre.Utilisateur),
            ChallengeEtapeId = etape.Id,
            NumeroEtape = etape.NumeroEtape,
            EtapeTitre = etape.TitreEtape,
            FormatAutorise = membre.Cohorte.Challenge.Format == FormatChallenge.BilanCompetencesIndividuel,
            CartesTemplateIds = etape.Cartes.Select(c => c.CarteCompetenceId).ToList(),
            CartesSupplementaires = supplementaires.Select(cs => new CarteSupplementaireInfo
            {
                CarteCompetenceId = cs.CarteCompetenceId,
                CarteCode = cs.CarteCompetence.Code,
                CarteTitre = cs.CarteCompetence.TitreTheorie,
                AjouteeParNomComplet = NomComplet(cs.AjouteePar),
                AjouteeLe = cs.AjouteeLe,
            }).ToList(),
            DefiIndividuelPartage = etape.DefiIndividuel,
            DefiIndividuelPersonnalise = instructionsPersonnalisees,
        };
    }

    // Remplacement total (comme ChallengeService.DefinirCartesEtapeAsync sur le template
    // partage), volontairement SANS verrou d'architecture : la personnalisation individuelle
    // doit rester modifiable meme une fois la Cohorte Active, le diagnostic d'un bilan de
    // competences pouvant evoluer en cours de parcours.
    public async Task<(bool Success, string? ErrorMessage)> DefinirCartesSupplementairesMembreAsync(int cohorteMembreId, int challengeEtapeId, List<int> carteCompetenceIds, string gestionnaireId)
    {
        var membre = await dbContext.CohorteMembres
            .Include(m => m.Cohorte)
                .ThenInclude(c => c.Challenge)
            .FirstOrDefaultAsync(m => m.Id == cohorteMembreId);

        if (membre is null)
        {
            return (false, "Membre introuvable.");
        }

        // Verification serveur, jamais seulement masquee cote UI (cf. CLAUDE.md, principe
        // Manifeste "l'equipe avant l'individu") : un Challenge Collectif ne doit jamais
        // recevoir de carte personnalisee par membre, la cohesion du groupe repose sur des
        // cartes partagees par toute la Cohorte.
        if (membre.Cohorte.Challenge.Format != FormatChallenge.BilanCompetencesIndividuel)
        {
            return (false, "Cette fonctionnalité est réservée aux parcours de type \"Bilan de compétences individuel\".");
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.Id == challengeEtapeId && e.ChallengeId == membre.Cohorte.ChallengeId);
        if (etape is null)
        {
            return (false, "Étape introuvable pour ce Challenge.");
        }

        var existantes = await dbContext.CohorteMembreCartesSupplementaires
            .Where(cs => cs.CohorteMembreId == cohorteMembreId && cs.ChallengeEtapeId == challengeEtapeId)
            .ToListAsync();

        var idsSouhaites = carteCompetenceIds.Distinct().ToList();
        var idsActuels = existantes.Select(cs => cs.CarteCompetenceId).ToList();

        var aRetirer = existantes.Where(cs => !idsSouhaites.Contains(cs.CarteCompetenceId)).ToList();
        foreach (var carte in aRetirer)
        {
            dbContext.CohorteMembreCartesSupplementaires.Remove(carte);
        }

        var aAjouter = idsSouhaites.Except(idsActuels);
        foreach (var carteId in aAjouter)
        {
            dbContext.CohorteMembreCartesSupplementaires.Add(new CohorteMembreCarteSupplementaire
            {
                CohorteMembreId = cohorteMembreId,
                ChallengeEtapeId = challengeEtapeId,
                CarteCompetenceId = carteId,
                AjouteeParId = gestionnaireId,
                AjouteeLe = DateTime.UtcNow,
            });
        }

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    // Remplacement pour ce membre uniquement (jamais de modification du template partage
    // ChallengeEtape.DefiIndividuel, qui continue de s'afficher tel quel pour tous les autres
    // membres/Cohortes issus du meme Challenge) - meme garde-fou de Format que
    // DefinirCartesSupplementairesMembreAsync.
    public async Task<(bool Success, string? ErrorMessage)> DefinirInstructionsPersonnaliseesMembreAsync(int cohorteMembreId, int challengeEtapeId, string? instructionsPersonnalisees, string gestionnaireId)
    {
        var membre = await dbContext.CohorteMembres
            .Include(m => m.Cohorte)
                .ThenInclude(c => c.Challenge)
            .FirstOrDefaultAsync(m => m.Id == cohorteMembreId);

        if (membre is null)
        {
            return (false, "Membre introuvable.");
        }

        if (membre.Cohorte.Challenge.Format != FormatChallenge.BilanCompetencesIndividuel)
        {
            return (false, "Cette fonctionnalité est réservée aux parcours de type \"Bilan de compétences individuel\".");
        }

        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.Id == challengeEtapeId && e.ChallengeId == membre.Cohorte.ChallengeId);
        if (etape is null)
        {
            return (false, "Étape introuvable pour ce Challenge.");
        }

        var existante = await dbContext.CohorteMembreEtapePersonnalisations
            .FirstOrDefaultAsync(p => p.CohorteMembreId == cohorteMembreId && p.ChallengeEtapeId == challengeEtapeId);

        if (string.IsNullOrWhiteSpace(instructionsPersonnalisees))
        {
            // Texte vide = retour au texte partage du template, pas une "instruction vide" a
            // afficher a l'apprenant.
            if (existante is not null)
            {
                dbContext.CohorteMembreEtapePersonnalisations.Remove(existante);
                await dbContext.SaveChangesAsync();
            }

            return (true, null);
        }

        if (existante is null)
        {
            dbContext.CohorteMembreEtapePersonnalisations.Add(new CohorteMembreEtapePersonnalisation
            {
                CohorteMembreId = cohorteMembreId,
                ChallengeEtapeId = challengeEtapeId,
                DefiIndividuelPersonnalise = instructionsPersonnalisees.Trim(),
                ModifieParId = gestionnaireId,
                ModifieLe = DateTime.UtcNow,
            });
        }
        else
        {
            existante.DefiIndividuelPersonnalise = instructionsPersonnalisees.Trim();
            existante.ModifieParId = gestionnaireId;
            existante.ModifieLe = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> SupprimerAsync(int id)
    {
        var cohorte = await dbContext.Cohortes.FirstOrDefaultAsync(c => c.Id == id);
        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.EnPreparation)
        {
            return (false, "Impossible de supprimer une Cohorte déjà lancée : des cartes ont pu être attribuées et des validations d'étape enregistrées, elles doivent rester traçables.");
        }

        // Tant qu'elle est En preparation, aucune attribution de carte ni validation d'etape
        // n'a pu avoir lieu via cette Cohorte (les deux ne se declenchent qu'a LancerAsync) :
        // les membres deja ajoutes (CohorteMembre -> Cohorte est en cascade) sont retires
        // sans rien perdre cote tracabilite.
        dbContext.Cohortes.Remove(cohorte);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    // ---- Embarquement ----

    public async Task<(bool Success, string? ErrorMessage, int? CohorteId)> DemanderEmbarquementAsync(int challengeId, string utilisateurId)
    {
        var challenge = await dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == challengeId);
        if (challenge is null)
        {
            return (false, "Challenge introuvable.", null);
        }

        if (challenge.Statut != StatutChallenge.Publie)
        {
            return (false, "Ce Challenge n'est pas encore publié.", null);
        }

        if (challenge.Mode != ModePlateforme.BtoC)
        {
            return (false, "La demande d'embarquement en libre-service n'est disponible que pour les Challenges BtoC.", null);
        }

        var cohorteProposee = await dbContext.Cohortes
            .FirstOrDefaultAsync(c => c.ChallengeId == challengeId && c.Statut == StatutCohorte.Proposee);

        if (cohorteProposee is null)
        {
            cohorteProposee = new Cohorte
            {
                ChallengeId = challengeId,
                Nom = $"Demande d'embarquement — {challenge.Titre}",
                Statut = StatutCohorte.Proposee,
                EtapeCourante = 0,
            };
            dbContext.Cohortes.Add(cohorteProposee);
            await dbContext.SaveChangesAsync();
        }

        if (await dbContext.CohorteMembres.AnyAsync(m => m.CohorteId == cohorteProposee.Id && m.UtilisateurId == utilisateurId))
        {
            return (true, null, cohorteProposee.Id);
        }

        dbContext.CohorteMembres.Add(new CohorteMembre
        {
            CohorteId = cohorteProposee.Id,
            UtilisateurId = utilisateurId,
            MethodeAjout = MethodeAjoutMembre.AutoInscription,
            DateAjout = DateTime.UtcNow,
        });
        await dbContext.SaveChangesAsync();

        return (true, null, cohorteProposee.Id);
    }

    public async Task<List<DemandeEmbarquementInfo>> GetDemandesEmbarquementAsync()
    {
        var demandes = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Membres)
            .Where(c => c.Statut == StatutCohorte.Proposee)
            .OrderBy(c => c.CreeLe)
            .ToListAsync();

        return demandes.Select(c => new DemandeEmbarquementInfo
        {
            CohorteId = c.Id,
            ChallengeId = c.ChallengeId,
            ChallengeTitre = c.Challenge.Titre,
            NombreDemandeurs = c.Membres.Count,
            DateCreation = c.CreeLe,
        }).ToList();
    }

    public async Task<(bool Success, string? ErrorMessage)> ValiderEmbarquementAsync(int cohorteId, string nom, DateTime dateLancement, string lienFormations)
    {
        if (string.IsNullOrWhiteSpace(nom))
        {
            return (false, "Le nom de la Cohorte est obligatoire.");
        }

        var cohorte = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Membres).ThenInclude(m => m.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == cohorteId);

        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.Proposee)
        {
            return (false, "Seule une demande d'embarquement en attente peut être validée.");
        }

        cohorte.Nom = nom.Trim();
        cohorte.DateLancement = dateLancement;
        cohorte.Statut = StatutCohorte.EnPreparation;
        await dbContext.SaveChangesAsync();

        var (sujet, corps) = ChallengeEmailTemplates.EmbarquementValide(cohorte.Challenge.Titre, cohorte.Nom, dateLancement, lienFormations);
        foreach (var membre in cohorte.Membres)
        {
            await notificationService.CreerAsync(membre.UtilisateurId, TypeNotification.DemandeEmbarquementValidee,
                ReferenceTypeNotification.Cohorte, cohorte.Id,
                $"Ta session pour \"{cohorte.Challenge.Titre}\" est confirmée — viens voir les détails", lienFormations);

            if (!string.IsNullOrWhiteSpace(membre.Utilisateur.Email))
            {
                await emailService.EnvoyerAsync(membre.Utilisateur.Email, sujet, corps);
            }
        }

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RefuserEmbarquementAsync(int cohorteId, string lienCatalogue)
    {
        var cohorte = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Membres)
            .FirstOrDefaultAsync(c => c.Id == cohorteId);

        if (cohorte is null)
        {
            return (false, "Cohorte introuvable.");
        }

        if (cohorte.Statut != StatutCohorte.Proposee)
        {
            return (false, "Seule une demande d'embarquement en attente peut être refusée.");
        }

        var demandeurIds = cohorte.Membres.Select(m => m.UtilisateurId).ToList();
        var challengeTitre = cohorte.Challenge.Titre;

        // Aucune trace utile a garder : une Cohorte Proposee n'a jamais eu de carte
        // attribuee ni d'etape validee (cf. SupprimerAsync, meme raisonnement).
        dbContext.Cohortes.Remove(cohorte);
        await dbContext.SaveChangesAsync();

        foreach (var demandeurId in demandeurIds)
        {
            await notificationService.CreerAsync(demandeurId, TypeNotification.DemandeEmbarquementRefusee,
                ReferenceTypeNotification.Cohorte, cohorteId,
                $"Ta demande d'embarquement pour \"{challengeTitre}\" n'a pas été retenue pour le moment.", lienCatalogue);
        }

        return (true, null);
    }

    // ---- Helpers ----

    private static string? VerifierInscriptionPossible(Cohorte cohorte)
    {
        return cohorte.Statut switch
        {
            StatutCohorte.Terminee => "Cette Cohorte est terminée : impossible d'y ajouter des membres.",
            StatutCohorte.Proposee => "Cette Cohorte est encore à l'état de demande, en attente de validation : impossible d'y ajouter des membres directement.",
            _ => null,
        };
    }

    // Vrai si l'etape n'a aucune carte attribuee (rien a emarger), ou si chaque carte
    // attribuee a une ligne Emargement signee - cf. ValiderEtapeAsync. Requete directement
    // les tables plutot que de dependre d'IEmargementService (meme raisonnement que pour
    // CarteAttribution ailleurs dans ce service : eviter un couplage circulaire entre
    // services pour une simple lecture).
    private async Task<bool> TousLesEmargementsSontSignesAsync(int cohorteId, int challengeId, int numeroEtape)
    {
        var etape = await dbContext.ChallengeEtapes
            .FirstOrDefaultAsync(e => e.ChallengeId == challengeId && e.NumeroEtape == numeroEtape);
        if (etape is null)
        {
            return true;
        }

        var attributionIds = await dbContext.CarteAttributions
            .Where(a => a.CohorteId == cohorteId && a.ChallengeEtapeId == etape.Id && a.EstActif)
            .Select(a => a.Id)
            .ToListAsync();

        if (attributionIds.Count == 0)
        {
            return true;
        }

        var nombreSignees = await dbContext.Emargements
            .CountAsync(e => attributionIds.Contains(e.CarteAttributionId) && e.SigneLe != null);

        return nombreSignees >= attributionIds.Count;
    }

    // Renvoie le type de test de connaissances qui bloque encore la validation de l'etape
    // en cours (Amont a l'etape 1, Aval a la derniere etape), ou null si rien ne bloque.
    // "Incomplet" recouvre aussi bien "jamais envoye" que "envoye mais tout le monde n'a pas
    // repondu" - dans les deux cas, la campagne doit etre (re)lancee par le Gestionnaire
    // avant de pouvoir avancer, cf. TestPositionnementService.
    private async Task<TypeTestPositionnement?> GetTestRequisManquantAsync(Cohorte cohorte)
    {
        var aDesEtapes = await dbContext.ChallengeEtapes.AnyAsync(e => e.ChallengeId == cohorte.ChallengeId);
        if (!aDesEtapes)
        {
            return null;
        }

        if (cohorte.EtapeCourante == 1 && !await TestPositionnementCompletAsync(cohorte.Id, TypeTestPositionnement.Amont))
        {
            return TypeTestPositionnement.Amont;
        }

        if (cohorte.EtapeCourante == cohorte.Challenge.NombreEtapes && !await TestPositionnementCompletAsync(cohorte.Id, TypeTestPositionnement.Aval))
        {
            return TypeTestPositionnement.Aval;
        }

        return null;
    }

    private async Task<bool> TestPositionnementCompletAsync(int cohorteId, TypeTestPositionnement type)
    {
        var test = await dbContext.TestsPositionnement.FirstOrDefaultAsync(t => t.CohorteId == cohorteId && t.Type == type);
        if (test is null)
        {
            return false;
        }

        var membreIds = await dbContext.CohorteMembres.Where(m => m.CohorteId == cohorteId).Select(m => m.UtilisateurId).ToListAsync();
        if (membreIds.Count == 0)
        {
            return true;
        }

        var repondantIds = await dbContext.TestsPositionnementReponses
            .Where(r => r.TestPositionnement.CohorteId == cohorteId && r.TestPositionnement.Type == type)
            .Select(r => r.UtilisateurId)
            .Distinct()
            .ToListAsync();

        return membreIds.All(repondantIds.Contains);
    }

    // Attribue les cartes de l'etape aux membres cibles (tous les membres actuels si
    // utilisateurIdsCibles est null). Idempotent : ne recree jamais une attribution deja
    // existante pour la meme (carte, utilisateur, cohorte, etape).
    private async Task AttribuerCartesEtapeAsync(Cohorte cohorte, int numeroEtape, string attribueParId, List<string>? utilisateurIdsCibles = null)
    {
        var etape = await dbContext.ChallengeEtapes
            .Include(e => e.Cartes)
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == numeroEtape);

        if (etape is null || etape.Cartes.Count == 0)
        {
            return;
        }

        var membreIds = utilisateurIdsCibles
            ?? await dbContext.CohorteMembres.Where(m => m.CohorteId == cohorte.Id).Select(m => m.UtilisateurId).ToListAsync();

        if (membreIds.Count == 0)
        {
            return;
        }

        var carteIds = etape.Cartes.Select(c => c.CarteCompetenceId).ToList();

        var existantes = await dbContext.CarteAttributions
            .Where(a => a.CohorteId == cohorte.Id
                && a.ChallengeEtapeId == etape.Id
                && carteIds.Contains(a.CarteCompetenceId)
                && membreIds.Contains(a.UtilisateurId))
            .Select(a => new { a.CarteCompetenceId, a.UtilisateurId })
            .ToListAsync();

        foreach (var carteId in carteIds)
        {
            foreach (var utilisateurId in membreIds)
            {
                if (existantes.Any(e => e.CarteCompetenceId == carteId && e.UtilisateurId == utilisateurId))
                {
                    continue;
                }

                dbContext.CarteAttributions.Add(new CarteAttribution
                {
                    CarteCompetenceId = carteId,
                    UtilisateurId = utilisateurId,
                    AttribueParId = attribueParId,
                    AttribueLe = DateTime.UtcNow,
                    Contexte = $"{cohorte.Challenge.Titre} — Étape {numeroEtape}",
                    EstActif = true,
                    OrigineType = OrigineAttribution.Challenge,
                    CohorteId = cohorte.Id,
                    ChallengeEtapeId = etape.Id,
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    // Rattrapage : un membre ajoute apres coup a une Cohorte deja Active recoit les
    // cartes de toutes les etapes deja validees jusqu'a l'etape courante (pas seulement
    // l'etape courante), pour ne jamais perdre l'acces aux cartes precedentes.
    private async Task RattraperCartesMembreAsync(Cohorte cohorte, string utilisateurId, string attribueParId)
    {
        for (var numero = 1; numero <= cohorte.EtapeCourante; numero++)
        {
            await AttribuerCartesEtapeAsync(cohorte, numero, attribueParId, [utilisateurId]);
        }
    }

    private async Task NotifierNouvelleEtapeAsync(Cohorte cohorte, int numeroEtape, string lienMonParcours)
    {
        var etape = await dbContext.ChallengeEtapes
            .Include(e => e.Cartes)
                .ThenInclude(ec => ec.CarteCompetence)
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == numeroEtape);

        if (etape is null)
        {
            return;
        }

        var carteTitres = etape.Cartes.Select(ec => ec.CarteCompetence.TitreTheorie).ToList();
        var (sujet, corps) = ChallengeEmailTemplates.NouvelleEtape(cohorte.Challenge.Titre, etape.TitreEtape, carteTitres, lienMonParcours);

        await EnvoyerATousLesMembresAsync(cohorte.Id, sujet, corps);
    }

    // Distinct de NotifierNouvelleEtapeAsync : un stagiaire qui rejoint n'a jamais utilise
    // la plateforme, il a besoin qu'on lui explique le principe (CBL, cartes, preuves,
    // validation par les pairs) avant de lui presenter la premiere etape - cf.
    // ChallengeEmailTemplates.LancementParcours.
    private async Task NotifierLancementAsync(Cohorte cohorte, string lienMonParcours)
    {
        var etape = await dbContext.ChallengeEtapes
            .Include(e => e.Cartes)
                .ThenInclude(ec => ec.CarteCompetence)
            .FirstOrDefaultAsync(e => e.ChallengeId == cohorte.ChallengeId && e.NumeroEtape == 1);

        if (etape is null)
        {
            return;
        }

        var carteTitres = etape.Cartes.Select(ec => ec.CarteCompetence.TitreTheorie).ToList();
        var (sujet, corps) = ChallengeEmailTemplates.LancementParcours(cohorte.Challenge.Titre, etape.TitreEtape, carteTitres, lienMonParcours);

        await EnvoyerATousLesMembresAsync(cohorte.Id, sujet, corps);
    }

    private async Task NotifierClotureAsync(Cohorte cohorte, string lienBibliotheque, string lienAttestation)
    {
        var lienAttestationComplet = $"{lienAttestation}?cohorteId={cohorte.Id}";
        var (sujet, corps) = ChallengeEmailTemplates.Cloture(cohorte.Challenge.Titre, lienBibliotheque, lienAttestationComplet);
        await EnvoyerATousLesMembresAsync(cohorte.Id, sujet, corps);
    }

    // Critere 7 Qualiopi (recueil des appreciations) : sollicitation unique a la cloture,
    // cf. ISatisfactionService - le lien pointe vers une page qui refuse silencieusement
    // toute deuxieme reponse (index unique CohorteId/UtilisateurId).
    private async Task NotifierDemandeSatisfactionAsync(Cohorte cohorte, string lienSatisfaction)
    {
        var lienComplet = $"{lienSatisfaction}?cohorteId={cohorte.Id}";
        var (sujet, corps) = ChallengeEmailTemplates.DemandeSatisfaction(cohorte.Challenge.Titre, lienComplet);
        await EnvoyerATousLesMembresAsync(cohorte.Id, sujet, corps);
    }

    // Point d'etape a mi-parcours : envoye automatiquement, une seule fois, des que la
    // Cohorte atteint l'etape mediane de son Challenge (arrondi au superieur : 10 etapes ->
    // etape 5, 9 etapes -> etape 5). Aucune sollicitation pour un Challenge a une seule
    // etape (pas de "milieu" qui ait du sens) - generique, pas de branche par type de
    // Challenge, cf. IQuestionnaireMiParcoursService.
    private async Task EnvoyerQuestionnaireMiParcoursSiEtapeMedianeAsync(Cohorte cohorte, int numeroEtape, string lienQuestionnaireMiParcours)
    {
        if (cohorte.Challenge.NombreEtapes < 2)
        {
            return;
        }

        var etapeMediane = (int)Math.Ceiling(cohorte.Challenge.NombreEtapes / 2.0);
        if (numeroEtape != etapeMediane)
        {
            return;
        }

        var lienComplet = $"{lienQuestionnaireMiParcours}?cohorteId={cohorte.Id}";
        var (sujet, corps) = ChallengeEmailTemplates.DemandeQuestionnaireMiParcours(cohorte.Challenge.Titre, lienComplet);
        await EnvoyerATousLesMembresAsync(cohorte.Id, sujet, corps);
    }

    private async Task EnvoyerATousLesMembresAsync(int cohorteId, string sujet, string corpsHtml)
    {
        var emails = await dbContext.CohorteMembres
            .Where(m => m.CohorteId == cohorteId)
            .Select(m => m.Utilisateur.Email)
            .Where(email => email != null)
            .ToListAsync();

        foreach (var email in emails)
        {
            await emailService.EnvoyerAsync(email!, sujet, corpsHtml);
        }
    }

    private async Task CreerEtEnvoyerInvitationAsync(ApplicationUser utilisateur, Func<string, string> construireLienActivation)
    {
        var invitation = new InvitationCompte
        {
            UtilisateurId = utilisateur.Id,
            Token = Guid.NewGuid().ToString("N"),
            CreeLe = DateTime.UtcNow,
            ExpireLe = DateTime.UtcNow.AddDays(7),
            EstActif = true,
        };

        dbContext.InvitationsComptes.Add(invitation);
        await dbContext.SaveChangesAsync();

        var lienActivation = construireLienActivation(invitation.Token);
        var (sujet, corps) = ChallengeEmailTemplates.InvitationDefinirMotDePasse(lienActivation);

        if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }
    }

    private static CohorteResume VersResume(Cohorte c) => new()
    {
        Id = c.Id,
        ChallengeId = c.ChallengeId,
        ChallengeTitre = c.Challenge.Titre,
        ChallengeMode = c.Challenge.Mode,
        Nom = c.Nom,
        DateLancement = c.DateLancement,
        EtapeCourante = c.EtapeCourante,
        NombreEtapes = c.Challenge.NombreEtapes,
        Statut = c.Statut,
        NombreMembres = c.Membres.Count,
        OrganisationId = c.OrganisationId,
        OrganisationNom = c.Organisation?.RaisonSociale,
        Format = c.Challenge.Format,
    };

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}
