using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class CohorteService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService) : ICohorteService
{
    public async Task<List<CohorteResume>> GetAllAsync()
    {
        var cohortes = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Organisation)
            .Include(c => c.Membres)
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync();

        return cohortes.Select(VersResume).ToList();
    }

    public async Task<CohorteResume?> GetResumeAsync(int id)
    {
        var cohorte = await dbContext.Cohortes
            .Include(c => c.Challenge)
            .Include(c => c.Organisation)
            .Include(c => c.Membres)
            .FirstOrDefaultAsync(c => c.Id == id);

        return cohorte is null ? null : VersResume(cohorte);
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

    public async Task<(bool Success, string? ErrorMessage)> LancerAsync(int cohorteId, string gestionnaireId, string lienMonParcours)
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
        await NotifierNouvelleEtapeAsync(cohorte, 1, lienMonParcours);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ValiderEtapeAsync(
        int cohorteId,
        string gestionnaireId,
        string lienMonParcours,
        string lienBibliotheque)
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

        var etapeValidee = cohorte.EtapeCourante;

        dbContext.CohorteEtapeValidations.Add(new CohorteEtapeValidation
        {
            CohorteId = cohorteId,
            NumeroEtape = etapeValidee,
            ValideParId = gestionnaireId,
            ValideLe = DateTime.UtcNow,
        });

        if (cohorte.EtapeCourante >= cohorte.Challenge.NombreEtapes)
        {
            cohorte.Statut = StatutCohorte.Terminee;
            await dbContext.SaveChangesAsync();

            await NotifierClotureAsync(cohorte, lienBibliotheque);
            return (true, null);
        }

        cohorte.EtapeCourante++;
        await dbContext.SaveChangesAsync();

        await AttribuerCartesEtapeAsync(cohorte, cohorte.EtapeCourante, gestionnaireId);
        await NotifierNouvelleEtapeAsync(cohorte, cohorte.EtapeCourante, lienMonParcours);

        return (true, null);
    }

    // ---- Helpers ----

    private static string? VerifierInscriptionPossible(Cohorte cohorte)
    {
        return cohorte.Statut switch
        {
            StatutCohorte.Terminee => "Cette Cohorte est terminée : impossible d'y ajouter des membres.",
            _ => null,
        };
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

    private async Task NotifierClotureAsync(Cohorte cohorte, string lienBibliotheque)
    {
        var (sujet, corps) = ChallengeEmailTemplates.Cloture(cohorte.Challenge.Titre, lienBibliotheque);
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
    };

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}
