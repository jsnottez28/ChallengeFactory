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
    IEmailService emailService,
    IPreuveFichierStockageService stockageService) : ICollecteCarboneService
{
    public async Task<DossierCarboneInfo?> GetDossierParPreBilanAsync(int preBilanId)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);
        return dossier is null ? null : await VersInfoAsync(dossier);
    }

    public Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientAsync(int preBilanId, string gestionnaireId, Func<string, string> construireLienActivation, string lienConnexion) =>
        OuvrirCompteClientInterneAsync(preBilanId, gestionnaireId, construireLienActivation, lienConnexion);

    // Self-service : le prospect ouvre lui-meme son espace juste apres avoir depose son
    // Pre-diagnostic, sans attendre un Gestionnaire. OuvertParId reste null (aucun
    // Gestionnaire n'est a l'origine de l'ouverture) - cf. DossierCarbone.OuvertParId.
    public Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientSelfServiceAsync(int preBilanId, Func<string, string> construireLienActivation, string lienConnexion) =>
        OuvrirCompteClientInterneAsync(preBilanId, ouvertParId: null, construireLienActivation, lienConnexion);

    private async Task<(bool Success, string? ErrorMessage)> OuvrirCompteClientInterneAsync(int preBilanId, string? ouvertParId, Func<string, string> construireLienActivation, string lienConnexion)
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

        // Compte client deja existant (autre dossier, ou compte Participant, avec mot de
        // passe deja defini) : pas de nouvelle invitation (le client a deja ses identifiants),
        // mais on le previent quand meme que son espace Bilan Carbone(R) est pret, sinon il
        // n'en sait jamais rien.
        if (compteCree || !await userManager.HasPasswordAsync(utilisateur))
        {
            await EnvoyerInvitationAsync(utilisateur, dossier.RaisonSociale, construireLienActivation);
        }
        else
        {
            await EnvoyerNotificationEspaceOuvertAsync(utilisateur, dossier.RaisonSociale, lienConnexion);
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

    public async Task<(bool Success, string? ErrorMessage)> RenvoyerEmailOuvertureAsync(int preBilanId, Func<string, string> construireLienActivation, string lienConnexion)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);
        if (dossier is null)
        {
            return (false, "Aucun espace client n'est ouvert pour ce pré-diagnostic.");
        }

        if (!await userManager.HasPasswordAsync(dossier.ClientUtilisateur))
        {
            return await RenvoyerInvitationAsync(preBilanId, construireLienActivation);
        }

        await EnvoyerNotificationEspaceOuvertAsync(dossier.ClientUtilisateur, dossier.RaisonSociale, lienConnexion);
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
        // Le plus recent si le meme compte porte plusieurs dossiers (ex. un second
        // pre-diagnostic de la meme entreprise l'annee suivante, ou un compte collaborateur
        // sur plusieurs dossiers).
        var dossier = await AccessiblePar(RequeteDossiers(), clientUtilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();

        return dossier is null ? null : await VersInfoAsync(dossier);
    }

    public async Task<(bool Success, string? ErrorMessage)> EnregistrerReponsesAsync(string clientUtilisateurId, Dictionary<string, string> reponses, bool soumettre)
    {
        var dossier = await AccessiblePar(RequeteDossiers(), clientUtilisateurId)
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

        var elements = ChecklistCollecteBuilder.Construire(dossier.PreBilanCarbone, LireReponses(dossier.Questionnaire));
        var coches = LireElementsCoches(dossier.ElementsCochesJson);
        var valeursSaisies = LireValeursSaisies(dossier.ValeursSaisiesJson);
        foreach (var element in elements)
        {
            element.EstCoche = coches.Contains(element.Cle);
            if (valeursSaisies.TryGetValue(element.Cle, out var valeurFichier))
            {
                element.ValeurFichier = valeurFichier;
            }
        }

        return elements;
    }

    public async Task<(bool Success, string? ErrorMessage)> EnregistrerElementsCochesAsync(string clientUtilisateurId, IReadOnlyCollection<string> elementsCoches)
    {
        var dossier = await AccessiblePar(dbContext.DossiersCarbone, clientUtilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();
        if (dossier is null)
        {
            return (false, "Aucun espace client Bilan Carbone® ne vous est associé.");
        }

        // Aucune verification que chaque cle correspond a un ElementCollecteInfo existant :
        // une cle "perimee" (poste/question retire du catalogue depuis) reste stockee sans
        // effet, comme pour QuestionnaireCollecte.ReponsesJson - jamais bloquant.
        dossier.ElementsCochesJson = JsonSerializer.Serialize(elementsCoches.Distinct());
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(byte[] Contenu, string NomFichier)?> GenererTableurAsync(int preBilanId, string? lienEspaceClient = null)
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
        var contenu = TableurCollecteGenerator.Generer(raisonSociale, DateTime.UtcNow, elements, lienEspaceClient);

        var nomSur = new string(raisonSociale.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray()).Trim('_');
        var nomFichier = $"Collecte_Bilan_Carbone_{(nomSur.Length > 0 ? nomSur : "client")}_{DateTime.Now:yyyyMMdd}.xlsx";

        return (contenu, nomFichier);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeposerTableurRempliAsync(string clientUtilisateurId, Stream contenu, string nomFichier, bool collecteDeclareeTerminee)
    {
        var dossier = await AccessiblePar(RequeteDossiers(), clientUtilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();
        var deposant = dossier is null ? null : await userManager.FindByIdAsync(clientUtilisateurId);

        return await DeposerTableurInterneAsync(
            dossier, contenu, nomFichier, collecteDeclareeTerminee, deposant?.Prenom ?? deposant?.Email,
            "Aucun espace client Bilan Carbone® ne vous est associé.");
    }

    // Cote Gestionnaire : saisie pour le compte du client (donnees recueillies par un autre
    // canal - visite terrain, appel telephonique...). Memes mecanismes d'extraction/fusion
    // que le depot client ; SaisiePar porte la mention "(consultant)" pour que le client et
    // ses collaborateurs comprennent d'ou vient la donnee. CollecteDeclareeTerminee n'est
    // jamais touche ici : c'est une declaration du client, pas du consultant.
    public async Task<(bool Success, string? ErrorMessage)> DeposerTableurRempliParGestionnaireAsync(int preBilanId, Stream contenu, string nomFichier, string gestionnaireNomAffiche)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);

        return await DeposerTableurInterneAsync(
            dossier, contenu, nomFichier, collecteDeclareeTerminee: null, $"{gestionnaireNomAffiche} (consultant)",
            "Ouvrez d'abord l'espace client.");
    }

    private async Task<(bool Success, string? ErrorMessage)> DeposerTableurInterneAsync(
        DossierCarbone? dossier, Stream contenu, string nomFichier, bool? collecteDeclareeTerminee, string? saisiePar, string messageDossierIntrouvable)
    {
        // RequeteDossiers() (pas une requete DossiersCarbone nue) : il faut PreBilanCarbone
        // + Questionnaire charges pour reconstruire la check-list (ChecklistCollecteBuilder)
        // et donc les cles connues servant a extraire les valeurs saisies ci-dessous.
        if (dossier is null)
        {
            return (false, messageDossierIntrouvable);
        }

        // Tamponne une seule fois : un flux sert a l'extraction (lecture XLWorkbook),
        // l'autre au stockage brut - XLWorkbook consomme le flux qu'on lui passe,
        // impossible de le reutiliser tel quel pour les deux usages.
        await using var tampon = new MemoryStream();
        await contenu.CopyToAsync(tampon);
        var octets = tampon.ToArray();

        var clesConnues = ChecklistCollecteBuilder.Construire(dossier.PreBilanCarbone, LireReponses(dossier.Questionnaire)).Select(e => e.Cle).ToHashSet();
        var maintenant = DateTime.UtcNow;
        var extraction = TableurCollecteValeursExtracteur.Extraire(new MemoryStream(octets), clesConnues, maintenant);
        if (extraction is null)
        {
            return (false, "Ce fichier n'a pas pu être lu comme un classeur Excel valide (.xlsx). Vérifiez qu'il s'agit bien du tableur de collecte téléchargé depuis l'espace, non modifié dans sa structure.");
        }

        if (!string.IsNullOrWhiteSpace(dossier.TableurDeposeCheminStockage))
        {
            // Un seul depot conserve : supprime l'ancien fichier avant d'enregistrer le
            // nouveau, pour ne pas accumuler des fichiers orphelins sur le stockage.
            await stockageService.SupprimerAsync(dossier.TableurDeposeCheminStockage);
        }

        var cheminStockage = await stockageService.EnregistrerAsync(new MemoryStream(octets), nomFichier);

        foreach (var valeur in extraction.Valeurs.Values)
        {
            valeur.SaisiePar = saisiePar;
        }

        // Extraction des valeurs reellement saisies (fusion incrementale avec le depot
        // precedent, et avec tout ce qui a deja ete saisi via le formulaire web) + des
        // lignes non reconnues (toujours remplacees par le dernier depot, pas cumulatif -
        // cf. DossierCarbone.LignesSupplementairesJson).
        var valeursFusionnees = FusionnerValeursSaisies(LireValeursSaisies(dossier.ValeursSaisiesJson), extraction.Valeurs);

        dossier.TableurDeposeCheminStockage = cheminStockage;
        dossier.TableurDeposeNomFichier = nomFichier;
        dossier.TableurDeposeLe = maintenant;
        dossier.TableurDeposeAvancementPourcent = CalculerAvancement(valeursFusionnees, clesConnues.Count);
        if (collecteDeclareeTerminee.HasValue)
        {
            dossier.CollecteDeclareeTerminee = collecteDeclareeTerminee.Value;
        }

        dossier.ValeursSaisiesJson = JsonSerializer.Serialize(valeursFusionnees);
        dossier.LignesSupplementairesJson = JsonSerializer.Serialize(extraction.LignesSupplementaires);

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> EnregistrerValeursFormulaireAsync(string utilisateurId, string nomAffiche, IReadOnlyList<(string Cle, ValeurSaisieFichier Valeur)> lignes)
    {
        var dossier = await AccessiblePar(RequeteDossiers(), utilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();

        return await EnregistrerValeursFormulaireInterneAsync(dossier, nomAffiche, lignes, "Aucun espace client Bilan Carbone® ne vous est associé.");
    }

    // Cote Gestionnaire : meme formulaire simple, pour le compte du client.
    public async Task<(bool Success, string? ErrorMessage)> EnregistrerValeursFormulaireParGestionnaireAsync(int preBilanId, string gestionnaireNomAffiche, IReadOnlyList<(string Cle, ValeurSaisieFichier Valeur)> lignes)
    {
        var dossier = await RequeteDossiers().FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);

        return await EnregistrerValeursFormulaireInterneAsync(dossier, $"{gestionnaireNomAffiche} (consultant)", lignes, "Ouvrez d'abord l'espace client.");
    }

    private async Task<(bool Success, string? ErrorMessage)> EnregistrerValeursFormulaireInterneAsync(
        DossierCarbone? dossier, string nomAffiche, IReadOnlyList<(string Cle, ValeurSaisieFichier Valeur)> lignes, string messageDossierIntrouvable)
    {
        if (dossier is null)
        {
            return (false, messageDossierIntrouvable);
        }

        var clesConnues = ChecklistCollecteBuilder.Construire(dossier.PreBilanCarbone, LireReponses(dossier.Questionnaire)).Select(e => e.Cle).ToHashSet();
        var valeurs = LireValeursSaisies(dossier.ValeursSaisiesJson);
        var maintenant = DateTime.UtcNow;

        foreach (var (cle, valeurBrute) in lignes)
        {
            if (!clesConnues.Contains(cle))
            {
                // Cle inconnue/perimee (poste retire du catalogue depuis l'affichage du
                // formulaire) : jamais bloquant, meme principe que EnregistrerElementsCochesAsync.
                continue;
            }

            // Normalise avant tout : un champ <input> laisse vide poste une chaine vide, pas
            // null - sans ca, le test "ligne vide" ci-dessous ne declencherait jamais.
            var valeur = new ValeurSaisieFichier
            {
                LibellePersonnalise = Normaliser(valeurBrute.LibellePersonnalise),
                Valeur = Normaliser(valeurBrute.Valeur),
                Unite = Normaliser(valeurBrute.Unite),
                Type = Normaliser(valeurBrute.Type),
                Source = Normaliser(valeurBrute.Source),
                Hypotheses = Normaliser(valeurBrute.Hypotheses),
                Statut = Normaliser(valeurBrute.Statut),
                Commentaire = Normaliser(valeurBrute.Commentaire),
            };

            var estVide = valeur.LibellePersonnalise is null && valeur.Valeur is null && valeur.Unite is null && valeur.Type is null
                && valeur.Source is null && valeur.Hypotheses is null && valeur.Statut is null && valeur.Commentaire is null;

            if (estVide)
            {
                // Soit la ligne n'a jamais ete touchee, soit l'utilisateur a deliberement
                // tout efface (contrairement au depot Excel, le formulaire est pre-rempli
                // avec les valeurs actuelles : un champ vide est une decision, pas une
                // absence de saisie) - dans les deux cas, rien a garder.
                valeurs.Remove(cle);
                continue;
            }

            valeur.MiseAJourLe = maintenant;
            valeur.SaisiePar = nomAffiche;
            valeurs[cle] = valeur;
        }

        dossier.ValeursSaisiesJson = JsonSerializer.Serialize(valeurs);
        dossier.TableurDeposeAvancementPourcent = CalculerAvancement(valeurs, clesConnues.Count);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> InviterCollaborateurAsync(int dossierId, string email, string inviteParId, Func<string, string> construireLienActivation, string lienConnexion)
    {
        var dossier = await dbContext.DossiersCarbone
            .Include(d => d.ClientUtilisateur)
            .Include(d => d.Collaborateurs)
            .FirstOrDefaultAsync(d => d.Id == dossierId);
        if (dossier is null)
        {
            return (false, "Dossier introuvable.");
        }

        var emailNormalise = email.Trim();
        if (string.Equals(emailNormalise, dossier.ClientUtilisateur.Email, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Cette personne est déjà le contact principal de ce dossier.");
        }

        var utilisateur = await userManager.FindByEmailAsync(emailNormalise);
        var compteCree = false;

        // Memes principes que OuvrirCompteClientInterneAsync : les acces se cumulent, un
        // compte existant (Participant ou deja collaborateur d'un autre dossier) recoit
        // simplement l'acces en plus, sans que son TypeCompte change.
        if (utilisateur is null)
        {
            utilisateur = new ApplicationUser
            {
                UserName = emailNormalise,
                Email = emailNormalise,
                Mode = ModePlateforme.BtoB,
                TypeCompte = TypeCompte.ClientCarbone,
                EmailConfirmed = true,
            };

            var resultat = await userManager.CreateAsync(utilisateur);
            if (!resultat.Succeeded)
            {
                return (false, string.Join(" ", resultat.Errors.Select(e => e.Description)));
            }

            compteCree = true;
        }
        else if (utilisateur.Id == dossier.ClientUtilisateurId || dossier.Collaborateurs.Any(c => c.UtilisateurId == utilisateur.Id))
        {
            return (false, "Cette personne a déjà accès à ce dossier.");
        }

        dbContext.DossierCarboneCollaborateurs.Add(new DossierCarboneCollaborateur
        {
            DossierCarboneId = dossier.Id,
            UtilisateurId = utilisateur.Id,
            InviteParId = inviteParId,
        });
        await dbContext.SaveChangesAsync();

        if (compteCree || !await userManager.HasPasswordAsync(utilisateur))
        {
            var inviteurInfo = await userManager.FindByIdAsync(inviteParId);
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

            if (!string.IsNullOrWhiteSpace(utilisateur.Email))
            {
                var (sujet, corps) = ChallengeEmailTemplates.InvitationCollaborateurCarbone(
                    utilisateur.Prenom, dossier.RaisonSociale, inviteurInfo?.Prenom ?? inviteurInfo?.Email, construireLienActivation(invitation.Token));
                await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
            }
        }
        else if (!string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            var inviteurInfo = await userManager.FindByIdAsync(inviteParId);
            var (sujet, corps) = ChallengeEmailTemplates.InvitationCollaborateurCarboneCompteExistant(
                utilisateur.Prenom, dossier.RaisonSociale, inviteurInfo?.Prenom ?? inviteurInfo?.Email, lienConnexion);
            await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
        }

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RetirerCollaborateurAsync(int dossierId, string utilisateurId)
    {
        var collaborateur = await dbContext.DossierCarboneCollaborateurs
            .FirstOrDefaultAsync(c => c.DossierCarboneId == dossierId && c.UtilisateurId == utilisateurId);
        if (collaborateur is null)
        {
            return (false, "Ce collaborateur n'a pas (ou plus) accès à ce dossier.");
        }

        dbContext.DossierCarboneCollaborateurs.Remove(collaborateur);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> EnregistrerLigneLibreAsync(string utilisateurId, string nomAffiche, LigneLibreCollecte ligne)
    {
        var dossier = await AccessiblePar(dbContext.DossiersCarbone, utilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();

        return await EnregistrerLigneLibreInterneAsync(dossier, nomAffiche, ligne, "Aucun espace client Bilan Carbone® ne vous est associé.");
    }

    public async Task<(bool Success, string? ErrorMessage)> EnregistrerLigneLibreParGestionnaireAsync(int preBilanId, string gestionnaireNomAffiche, LigneLibreCollecte ligne)
    {
        var dossier = await dbContext.DossiersCarbone.FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);

        return await EnregistrerLigneLibreInterneAsync(dossier, $"{gestionnaireNomAffiche} (consultant)", ligne, "Ouvrez d'abord l'espace client.");
    }

    private async Task<(bool Success, string? ErrorMessage)> EnregistrerLigneLibreInterneAsync(DossierCarbone? dossier, string nomAffiche, LigneLibreCollecte ligne, string messageDossierIntrouvable)
    {
        if (dossier is null)
        {
            return (false, messageDossierIntrouvable);
        }

        var donnee = Normaliser(ligne.Donnee);
        if (donnee is null)
        {
            return (false, "Merci de préciser ce que représente cette ligne.");
        }

        var lignes = LireLignesLibres(dossier.LignesLibresJson);
        var id = string.IsNullOrWhiteSpace(ligne.Id) ? Guid.NewGuid().ToString("N") : ligne.Id;

        lignes[id] = new LigneLibreCollecte
        {
            Id = id,
            Poste = ligne.Poste,
            Donnee = donnee,
            Valeur = Normaliser(ligne.Valeur),
            Unite = Normaliser(ligne.Unite),
            Type = Normaliser(ligne.Type),
            Source = Normaliser(ligne.Source),
            Hypotheses = Normaliser(ligne.Hypotheses),
            Statut = Normaliser(ligne.Statut),
            Commentaire = Normaliser(ligne.Commentaire),
            SaisiePar = nomAffiche,
            MiseAJourLe = DateTime.UtcNow,
        };

        dossier.LignesLibresJson = JsonSerializer.Serialize(lignes);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> SupprimerLigneLibreAsync(string utilisateurId, string ligneId)
    {
        var dossier = await AccessiblePar(dbContext.DossiersCarbone, utilisateurId)
            .OrderByDescending(d => d.OuvertLe)
            .FirstOrDefaultAsync();

        return await SupprimerLigneLibreInterneAsync(dossier, ligneId, "Aucun espace client Bilan Carbone® ne vous est associé.");
    }

    public async Task<(bool Success, string? ErrorMessage)> SupprimerLigneLibreParGestionnaireAsync(int preBilanId, string ligneId)
    {
        var dossier = await dbContext.DossiersCarbone.FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);

        return await SupprimerLigneLibreInterneAsync(dossier, ligneId, "Ouvrez d'abord l'espace client.");
    }

    private async Task<(bool Success, string? ErrorMessage)> SupprimerLigneLibreInterneAsync(DossierCarbone? dossier, string ligneId, string messageDossierIntrouvable)
    {
        if (dossier is null)
        {
            return (false, messageDossierIntrouvable);
        }

        var lignes = LireLignesLibres(dossier.LignesLibresJson);
        if (!lignes.Remove(ligneId))
        {
            return (false, "Cette ligne n'existe plus.");
        }

        dossier.LignesLibresJson = JsonSerializer.Serialize(lignes);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(Stream Contenu, string NomFichier)?> TelechargerDernierTableurDeposeAsync(int preBilanId)
    {
        var dossier = await dbContext.DossiersCarbone.FirstOrDefaultAsync(d => d.PreBilanCarboneId == preBilanId);
        if (dossier?.TableurDeposeCheminStockage is null)
        {
            return null;
        }

        var flux = await stockageService.TelechargerAsync(dossier.TableurDeposeCheminStockage);
        return flux is null ? null : (flux, dossier.TableurDeposeNomFichier ?? "tableur-collecte.xlsx");
    }

    private IQueryable<DossierCarbone> RequeteDossiers() => dbContext.DossiersCarbone
        .Include(d => d.ClientUtilisateur)
        .Include(d => d.Questionnaire)
        .Include(d => d.Collaborateurs).ThenInclude(c => c.Utilisateur)
        .Include(d => d.PreBilanCarbone).ThenInclude(p => p.Lignes).ThenInclude(l => l.FacteurEmission);

    // Un dossier est accessible a son contact principal ET a tous ses collaborateurs (acces
    // complet, pas de perimetre par poste - cf. DossierCarbone.Collaborateurs). A utiliser
    // partout ou un Id de dossier est resolu depuis l'utilisateur connecte, jamais depuis une
    // valeur fournie par le client.
    private static IQueryable<DossierCarbone> AccessiblePar(IQueryable<DossierCarbone> requete, string utilisateurId) =>
        requete.Where(d => d.ClientUtilisateurId == utilisateurId || d.Collaborateurs.Any(c => c.UtilisateurId == utilisateurId));

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

    private async Task EnvoyerNotificationEspaceOuvertAsync(ApplicationUser utilisateur, string raisonSociale, string lienConnexion)
    {
        if (string.IsNullOrWhiteSpace(utilisateur.Email))
        {
            return;
        }

        var (sujet, corps) = ChallengeEmailTemplates.EspaceClientCarboneOuvertCompteExistant(utilisateur.Prenom, raisonSociale, lienConnexion);
        await emailService.EnvoyerAsync(utilisateur.Email, sujet, corps);
    }

    private static string? Normaliser(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();

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

    private static HashSet<string> LireElementsCoches(string elementsCochesJson)
    {
        if (string.IsNullOrWhiteSpace(elementsCochesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<HashSet<string>>(elementsCochesJson) ?? [];
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
        TableurDeposeNomFichier = dossier.TableurDeposeNomFichier,
        TableurDeposeLe = dossier.TableurDeposeLe,
        TableurDeposeAvancementPourcent = dossier.TableurDeposeAvancementPourcent,
        CollecteDeclareeTerminee = dossier.CollecteDeclareeTerminee,
        LignesSupplementaires = LireLignesSupplementaires(dossier.LignesSupplementairesJson),
        LignesLibres = LireLignesLibres(dossier.LignesLibresJson).Values.OrderBy(l => l.MiseAJourLe).ToList(),
        Collaborateurs = dossier.Collaborateurs
            .OrderBy(c => c.InviteLe)
            .Select(c => new CollaborateurInfo
            {
                UtilisateurId = c.UtilisateurId,
                Email = c.Utilisateur.Email ?? string.Empty,
                Prenom = c.Utilisateur.Prenom,
                InviteLe = c.InviteLe,
            })
            .ToList(),
    };

    private static Dictionary<string, ValeurSaisieFichier> LireValeursSaisies(string valeursSaisiesJson)
    {
        if (string.IsNullOrWhiteSpace(valeursSaisiesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, ValeurSaisieFichier>>(valeursSaisiesJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static Dictionary<string, LigneLibreCollecte> LireLignesLibres(string lignesLibresJson)
    {
        if (string.IsNullOrWhiteSpace(lignesLibresJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, LigneLibreCollecte>>(lignesLibresJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<LigneSupplementaireCollecte> LireLignesSupplementaires(string lignesSupplementairesJson)
    {
        if (string.IsNullOrWhiteSpace(lignesSupplementairesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<LigneSupplementaireCollecte>>(lignesSupplementairesJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    // Fusion champ par champ : une cellule vide dans le nouveau depot ne remplace jamais une
    // valeur deja enregistree (cf. DossierCarbone.ValeursSaisiesJson) - seule une cellule
    // effectivement renseignee ecrase l'ancienne valeur de CE champ precis.
    private static Dictionary<string, ValeurSaisieFichier> FusionnerValeursSaisies(
        Dictionary<string, ValeurSaisieFichier> existantes, Dictionary<string, ValeurSaisieFichier> nouvelles)
    {
        var resultat = new Dictionary<string, ValeurSaisieFichier>(existantes);
        foreach (var (cle, nouvelle) in nouvelles)
        {
            resultat[cle] = existantes.TryGetValue(cle, out var ancienne)
                ? new ValeurSaisieFichier
                {
                    LibellePersonnalise = nouvelle.LibellePersonnalise ?? ancienne.LibellePersonnalise,
                    Valeur = nouvelle.Valeur ?? ancienne.Valeur,
                    Unite = nouvelle.Unite ?? ancienne.Unite,
                    Type = nouvelle.Type ?? ancienne.Type,
                    Source = nouvelle.Source ?? ancienne.Source,
                    Hypotheses = nouvelle.Hypotheses ?? ancienne.Hypotheses,
                    Statut = nouvelle.Statut ?? ancienne.Statut,
                    Commentaire = nouvelle.Commentaire ?? ancienne.Commentaire,
                    MiseAJourLe = nouvelle.MiseAJourLe,
                    SaisiePar = nouvelle.SaisiePar ?? ancienne.SaisiePar,
                }
                : nouvelle;
        }

        return resultat;
    }

    // % des elements connus dont le Statut saisi (formulaire ou fichier) vaut Recu/Valide -
    // calcule sur l'ensemble fusionne, jamais sur un seul canal de saisie : un client qui
    // commence par le formulaire web puis finit sur Excel (ou l'inverse) voit une jauge
    // cumulee, pas remise a zero en changeant de canal.
    private static int CalculerAvancement(Dictionary<string, ValeurSaisieFichier> valeurs, int totalElementsConnus) =>
        totalElementsConnus == 0 ? 0 : (int)Math.Round(valeurs.Values.Count(v => v.Statut is "Reçu" or "Validé") * 100.0 / totalElementsConnus);
}
