using System.Text.Json;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class CollecteCarboneService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService) : ICollecteCarboneService
{
    public async Task<DossierCarboneInfo?> GetDossierParPreBilanAsync(int preBilanId)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);
        return dossier is null ? null : await VersInfoAsync(dossier);
    }

    public Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientAsync(int preBilanId, string gestionnaireId, Func<string, string> construireLienActivation) =>
        OuvrirCompteClientInterneAsync(preBilanId, gestionnaireId, construireLienActivation);

    // Self-service : le prospect ouvre lui-meme son espace juste apres avoir depose son
    // Pre-diagnostic, sans attendre un Gestionnaire. OuvertParId reste null (aucun
    // Gestionnaire n'est a l'origine de l'ouverture) - cf. DossierCarbone.OuvertParId.
    public Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientSelfServiceAsync(int preBilanId, Func<string, string> construireLienActivation) =>
        OuvrirCompteClientInterneAsync(preBilanId, ouvertParId: null, construireLienActivation);

    private async Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientInterneAsync(int preBilanId, string? ouvertParId, Func<string, string> construireLienActivation)
    {
        var preBilan = await dbContext.PreBilansCarbone.FirstOrDefaultAsync(p => p.Id == preBilanId);
        if (preBilan is null)
        {
            return (false, "Pré-diagnostic introuvable.");
        }

        if (await dbContext.DossiersCarbone.AnyAsync(d => d.PreBilanCarboneId == preBilanId))
        {
            return (false, "Un espace client est déjà ouvert pour ce pré-diagnostic.");
        }

        var email = preBilan.Email.Trim();
        var utilisateur = await userManager.FindByEmailAsync(email);
        var compteCree = false;

        // Les acces se cumulent : un compte Participant existant peut tout a fait recevoir
        // en plus un Espace Client Carbone, sans que son TypeCompte change (cf.
        // Domain.Entities.TypeCompte - ce champ ne reflete que la nature du compte a sa
        // creation, pas la liste de ce a quoi il a acces). La seule source de verite pour
        // "cette personne a acces a l'Espace Client Carbone" est la presence d'un
        // DossierCarbone qui lui est rattache, jamais TypeCompte.
        if (utilisateur is null)
        {
            utilisateur = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Prenom = preBilan.Prenom,
                Nom = preBilan.Nom,
                Telephone = preBilan.Telephone,
                Mode = ModePlateforme.BtoB,
                TypeCompte = TypeCompte.ClientCarbone,
                // Confirme d'office : c'est le Gestionnaire (ou le prospect lui-meme en
                // self-service) qui ouvre le compte a partir d'un email deja fourni par le
                // prospect (meme principe que l'import de Cohorte, cf. CohorteService).
                EmailConfirmed = true,
            };

            var resultat = await userManager.CreateAsync(utilisateur);
            if (!resultat.Succeeded)
            {
                return (false, string.Join(" ", resultat.Errors.Select(e => e.Description)));
            }

            compteCree = true;
        }

        var dossier = new DossierCarbone
        {
            PreBilanCarboneId = preBilan.Id,
            ClientUtilisateurId = utilisateur.Id,
            RaisonSociale = string.IsNullOrWhiteSpace(preBilan.Societe) ? email : preBilan.Societe.Trim(),
            OuvertLe = DateTime.UtcNow,
            OuvertParId = ouvertParId,
        };
        dbContext.DossiersCarbone.Add(dossier);

        if (preBilan.StatutCrm is StatutPreBilanCrm.Nouveau or StatutPreBilanCrm.Contacte)
        {
            preBilan.StatutCrm = StatutPreBilanCrm.Qualifie;
        }

        await dbContext.SaveChangesAsync();

        // Compte client deja existant (autre dossier, mot de passe deja defini) : pas de
        // nouvelle invitation, l'email de bienvenue renvoie simplement vers la connexion.
        if (compteCree || !await userManager.HasPasswordAsync(utilisateur))
        {
            await EnvoyerInvitationAsync(utilisateur, dossier.RaisonSociale, construireLienActivation);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RenvoyerInvitationAsync(int preBilanId, Func<string, string> construireLienActivation)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);
        if (dossier is null)
        {
            return (false, "Aucun espace client n'est ouvert pour ce pré-diagnostic.");
        }

        if (await userManager.HasPasswordAsync(dossier.ClientUtilisateur))
        {
            return (false, "Le client a déjà défini son mot de passe : il peut se connecter (ou utiliser « Mot de passe oublié »).");
        }

        var anciennes = await dbContext.InvitationsComptes
            .Where(i => i.UtilisateurId == dossier.ClientUtilisateurId && i.EstActif)
            .ToListAsync();
        foreach (var ancienne in anciennes)
        {
            ancienne.EstActif = false;
        }

        await dbContext.SaveChangesAsync();
        await EnvoyerInvitationAsync(dossier.ClientUtilisateur, dossier.RaisonSociale, construireLienActivation);

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> EnvoyerQuestionnaireAsync(int preBilanId, string lienQuestionnaire)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);
        if (dossier is null)
        {
            return (false, "Ouvrez d'abord l'espace client.");
        }

        if (dossier.Questionnaire is null)
        {
            dossier.Questionnaire = new QuestionnaireCollecte
            {
                DossierCarboneId = dossier.Id,
                Statut = StatutQuestionnaireCollecte.Envoye,
                EnvoyeLe = DateTime.UtcNow,
            };
        }
        else
        {
            // Renvoi : on rouvre un questionnaire deja soumis (les reponses sont
            // conservees, le client les complete ou corrige).
            if (dossier.Questionnaire.Statut == StatutQuestionnaireCollecte.Soumis)
            {
                dossier.Questionnaire.Statut = StatutQuestionnaireCollecte.EnCours;
            }

            dossier.Questionnaire.EnvoyeLe = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(dossier.ClientUtilisateur.Email))
        {
            var (sujet, corps) = ChallengeEmailTemplates.QuestionnaireCollecteCarbone(dossier.ClientUtilisateur.Prenom, dossier.RaisonSociale, lienQuestionnaire);
            await emailService.EnvoyerAsync(dossier.ClientUtilisateur.Email, sujet, corps);
        }

        return (true, null);
    }

    public async Task<DossierCarboneInfo?> GetDossierClientAsync(string clientUtilisateurId)
    {
        // Le plus recent si le meme compte client porte plusieurs dossiers (ex. un second
        // pre-diagnostic de la meme entreprise l'annee suivante).
        var dossier = await RequeteDossiers()
            .Where(d => d.ClientUtilisateurId == clientUtilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();

        return dossier is null ? null : await VersInfoAsync(dossier);
    }

    public async Task<(bool Success, string? ErrorMessage)> EnregistrerReponsesAsync(string clientUtilisateurId, Dictionary<string, string> reponses, bool soumettre)
    {
        var dossier = await RequeteDossiers()
            .Where(d => d.ClientUtilisateurId == clientUtilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();

        var questionnaire = dossier?.Questionnaire;
        if (questionnaire is null)
        {
            return (false, "Aucun questionnaire ne vous a encore été envoyé.");
        }

        if (questionnaire.Statut == StatutQuestionnaireCollecte.Soumis)
        {
            return (false, "Ce questionnaire a déjà été envoyé. Contactez votre consultant pour le modifier.");
        }

        var propres = QuestionnaireCollecteCatalogue.Nettoyer(reponses);

        if (soumettre)
        {
            var manquantes = QuestionnaireCollecteCatalogue.Questions
                .Where(q => q.Type != TypeQuestionCollecte.Nombre && !propres.ContainsKey(q.Code))
                .ToList();
            if (manquantes.Count > 0)
            {
                // Le brouillon est quand meme conserve : le client ne perd pas sa saisie.
                questionnaire.ReponsesJson = JsonSerializer.Serialize(propres);
                questionnaire.Statut = StatutQuestionnaireCollecte.EnCours;
                questionnaire.ModifieLe = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                return (false, $"Merci de répondre à toutes les questions avant d'envoyer ({manquantes.Count} sans réponse). Vous pouvez choisir « {QuestionnaireCollecteCatalogue.NeSaitPas} ».");
            }
        }

        questionnaire.ReponsesJson = JsonSerializer.Serialize(propres);
        questionnaire.ModifieLe = DateTime.UtcNow;
        questionnaire.Statut = soumettre ? StatutQuestionnaireCollecte.Soumis : StatutQuestionnaireCollecte.EnCours;
        if (soumettre)
        {
            questionnaire.SoumisLe = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<ElementCollecteInfo>> GetChecklistAsync(int dossierId)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.Id == dossierId);
        if (dossier is null)
        {
            return [];
        }

        return ChecklistCollecteBuilder.Construire(dossier.PreBilanCarbone, LireReponses(dossier.Questionnaire));
    }

    public async Task<(byte[] Contenu, string NomFichier)?> GenererTableurAsync(int preBilanId)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);

        PreBilanCarbone? preBilan;
        string raisonSociale;
        Dictionary<string, string> reponses;

        if (dossier is not null)
        {
            preBilan = dossier.PreBilanCarbone;
            raisonSociale = dossier.RaisonSociale;
            reponses = LireReponses(dossier.Questionnaire);
        }
        else
        {
            // Sans espace client : tableur "socle + Pre-diagnostic" (utile pour un client
            // qui prefere travailler hors plateforme).
            preBilan = await dbContext.PreBilansCarbone
                .Include(p => p.Lignes).ThenInclude(l => l.FacteurEmission)
                .FirstOrDefaultAsync(p => p.Id == preBilanId);
            if (preBilan is null)
            {
                return null;
            }

            raisonSociale = string.IsNullOrWhiteSpace(preBilan.Societe) ? preBilan.Email : preBilan.Societe;
            reponses = [];
        }

        var elements = ChecklistCollecteBuilder.Construire(preBilan, reponses);
        var contenu = TableurCollecteGenerator.Generer(raisonSociale, DateTime.UtcNow, elements);

        var nomSur = new string(raisonSociale.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray()).Trim('_');
        var nomFichier = $"Collecte_Bilan_Carbone_{(nomSur.Length > 0 ? nomSur : "client")}_{DateTime.Now:yyyyMMdd}.xlsx";

        return (contenu, nomFichier);
    }

    private IQueryable<DossierCarbone> RequeteDossiers() => dbContext.DossiersCarbone
        .Include(d => d.ClientUtilisateur)
        .Include(d => d.Questionnaire)
        .Include(d => d.PreBilanCarbone).ThenInclude(p => p.Lignes).ThenInclude(l => l.FacteurEmission);

    private async Task EnvoyerInvitationAsync(ApplicationUser utilisateur, string raisonSociale, Func<string, string> construireLienActivation)
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

        if (string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            return;
        }

        var (sujet, corps) = ChallengeEmailTemplates.BienvenueEspaceClientCarbone(
            utilisateur.Prenom, utilisateur.Email, raisonSociale, construireLienActivation(invitation.Token));
        await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
    }

    private static Dictionary<string, string> LireReponses(QuestionnaireCollecte? questionnaire)
    {
        if (questionnaire is null || string.IsNullOrWhiteSpace(questionnaire.ReponsesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(questionnaire.ReponsesJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task<DossierCarboneInfo> VersInfoAsync(DossierCarbone dossier) => new()
    {
        Id = dossier.Id,
        PreBilanCarboneId = dossier.PreBilanCarboneId,
        RaisonSociale = dossier.RaisonSociale,
        OuvertLe = dossier.OuvertLe,
        ClientUtilisateurId = dossier.ClientUtilisateurId,
        ClientEmail = dossier.ClientUtilisateur.Email ?? string.Empty,
        ClientPrenom = dossier.ClientUtilisateur.Prenom,
        CompteActive = await userManager.HasPasswordAsync(dossier.ClientUtilisateur),
        StatutQuestionnaire = dossier.Questionnaire?.Statut,
        QuestionnaireEnvoyeLe = dossier.Questionnaire?.EnvoyeLe,
        QuestionnaireSoumisLe = dossier.Questionnaire?.SoumisLe,
        Reponses = LireReponses(dossier.Questionnaire),
    };
}
