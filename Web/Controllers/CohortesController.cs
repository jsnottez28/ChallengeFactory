using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Data;
using Web.Services;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class CohortesController(
    ICohorteService cohorteService,
    IChallengeService challengeService,
    IOrganisationService organisationService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:COHORTE.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var cohortes = await cohorteService.GetAllAsync();
        return View(cohortes);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:COHORTE.CREER")]
    public async Task<IActionResult> Create()
    {
        return View("Save", new CohorteFormModel
        {
            Challenges = await ListeChallengesPublies(),
            Organisations = await ListeOrganisations(),
        });
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:COHORTE.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CohorteFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Challenges = await ListeChallengesPublies();
            model.Organisations = await ListeOrganisations();
            return View("Save", model);
        }

        var (success, errorMessage, cohorteId) = await cohorteService.CreateAsync(new CohorteInput
        {
            ChallengeId = model.ChallengeId,
            Nom = model.Nom,
            DateLancement = model.DateLancement,
            OrganisationId = model.OrganisationId,
        });

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Nom), errorMessage ?? "Impossible de créer cette Cohorte.");
            model.Challenges = await ListeChallengesPublies();
            model.Organisations = await ListeOrganisations();
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Cohorte créée.";
        return RedirectToAction(nameof(Details), new { id = cohorteId });
    }

    [HttpGet("Details/{id:int}")]
    [Authorize(Policy = "Droit:COHORTE.CONSULTER")]
    public async Task<IActionResult> Details(int id)
    {
        var cohorte = await cohorteService.GetResumeAsync(id);
        if (cohorte is null)
        {
            return NotFound();
        }

        ViewData["Membres"] = await cohorteService.GetMembresAsync(id);
        ViewData["Historique"] = await cohorteService.GetHistoriqueValidationsAsync(id);
        ViewData["Visios"] = await cohorteService.GetVisiosAsync(id);

        return View(cohorte);
    }

    // ---- Lancement (planification de la visio de l'étape 1 obligatoire, prompt "Visio
    // planifiee par etape" section 1.2) ----

    [HttpGet("{id:int}/LancerConfirmation")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    public async Task<IActionResult> LancerConfirmation(int id)
    {
        var cohorte = await cohorteService.GetResumeAsync(id);
        if (cohorte is null || cohorte.Statut != StatutCohorte.EnPreparation)
        {
            return NotFound();
        }

        var challenge = await challengeService.GetByIdAsync(cohorte.ChallengeId);
        var etape1 = challenge?.Etapes.FirstOrDefault(e => e.NumeroEtape == 1);
        if (etape1 is null)
        {
            TempData["StatusMessage"] = "Étape 1 introuvable pour ce Challenge.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View("LancerConfirmation", new LancerFormModel
        {
            CohorteId = id,
            CohorteNom = cohorte.Nom,
            ChallengeTitre = cohorte.ChallengeTitre,
            TitreEtape = etape1.TitreEtape,
            DescriptifVisio = await cohorteService.GenererDescriptifVisioAsync(etape1.Id),
        });
    }

    [HttpPost("{id:int}/Lancer")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lancer(int id, LancerFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CohorteId = id;
            return View("LancerConfirmation", model);
        }

        var lienMonParcours = Url.Page("/Dashboard/MonParcours", null, null, Request.Scheme) ?? "/Dashboard/MonParcours";
        var (success, errorMessage) = await cohorteService.LancerAsync(
            id, userManager.GetUserId(User)!, lienMonParcours,
            model.DateHeureVisio, model.LienConnexionVisio, model.DescriptifVisio);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de lancer cette Cohorte.");
            model.CohorteId = id;
            return View("LancerConfirmation", model);
        }

        TempData["StatusMessage"] = "Cohorte lancée : étape 1 attribuée, visio planifiée et membres notifiés.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ---- Validation d'etape (planification de la visio de l'etape suivante obligatoire,
    // sauf sur la derniere etape qui cloture le Challenge - prompt section 1.2) ----

    [HttpGet("{id:int}/ValiderEtapeConfirmation")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    public async Task<IActionResult> ValiderEtapeConfirmation(int id)
    {
        var cohorte = await cohorteService.GetResumeAsync(id);
        if (cohorte is null || cohorte.Statut != StatutCohorte.Active)
        {
            return NotFound();
        }

        if (cohorte.EtapeCourante >= cohorte.NombreEtapes)
        {
            // Derniere etape : aucune visio a planifier, reste sur le flux direct existant
            // (bouton simple sur Details.cshtml).
            return RedirectToAction(nameof(Details), new { id });
        }

        var challenge = await challengeService.GetByIdAsync(cohorte.ChallengeId);
        var etapeSuivante = challenge?.Etapes.FirstOrDefault(e => e.NumeroEtape == cohorte.EtapeCourante + 1);
        if (etapeSuivante is null)
        {
            TempData["StatusMessage"] = "Étape suivante introuvable pour ce Challenge.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View("ValiderEtapeConfirmation", new ValiderEtapeFormModel
        {
            CohorteId = id,
            CohorteNom = cohorte.Nom,
            ChallengeTitre = cohorte.ChallengeTitre,
            NumeroEtapeSuivante = etapeSuivante.NumeroEtape,
            TitreEtapeSuivante = etapeSuivante.TitreEtape,
            DescriptifVisio = await cohorteService.GenererDescriptifVisioAsync(etapeSuivante.Id),
        });
    }

    [HttpPost("{id:int}/ValiderEtapeAvecVisio")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderEtapeAvecVisio(int id, ValiderEtapeFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CohorteId = id;
            return View("ValiderEtapeConfirmation", model);
        }

        var lienMonParcours = Url.Page("/Dashboard/MonParcours", null, null, Request.Scheme) ?? "/Dashboard/MonParcours";
        var lienBibliotheque = Url.Page("/Dashboard/Cartes", null, null, Request.Scheme) ?? "/Dashboard/Cartes";

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(
            id, userManager.GetUserId(User)!, lienMonParcours, lienBibliotheque,
            model.DateHeureVisio, model.LienConnexionVisio, model.DescriptifVisio);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de valider cette étape.");
            model.CohorteId = id;
            return View("ValiderEtapeConfirmation", model);
        }

        TempData["StatusMessage"] = "Étape validée, visio de l'étape suivante planifiée.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Flux direct conserve UNIQUEMENT pour la derniere etape (cloture du Challenge) : pas
    // de visio a planifier, donc pas de page de confirmation necessaire (prompt section
    // 1.2, "sur la derniere etape ce champ n'est pas demande"). La verification cote
    // service (ValiderEtapeAsync) protege quand meme contre un appel direct sur une etape
    // non-derniere : elle renvoie une erreur explicite plutot que d'ignorer la visio.
    [HttpPost("{id:int}/ValiderEtape")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderEtape(int id)
    {
        var lienMonParcours = Url.Page("/Dashboard/MonParcours", null, null, Request.Scheme) ?? "/Dashboard/MonParcours";
        var lienBibliotheque = Url.Page("/Dashboard/Cartes", null, null, Request.Scheme) ?? "/Dashboard/Cartes";

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(
            id, userManager.GetUserId(User)!, lienMonParcours, lienBibliotheque,
            null, null, null);

        TempData["StatusMessage"] = success ? "Étape validée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/Delete")]
    [Authorize(Policy = "Droit:COHORTE.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await cohorteService.SupprimerAsync(id);
        TempData["StatusMessage"] = success ? "Cohorte supprimée." : errorMessage;
        return success ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/Membres/AjouterManuel")]
    [Authorize(Policy = "Droit:COHORTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjouterMembreManuel(int id, string email)
    {
        var utilisateur = await userManager.FindByEmailAsync(email.Trim());
        if (utilisateur is null)
        {
            TempData["StatusMessage"] = $"Aucun compte existant pour {email}. Utilisez l'import pour créer un compte.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var (success, errorMessage) = await cohorteService.AjouterMembreManuelAsync(id, utilisateur.Id);
        TempData["StatusMessage"] = success ? "Membre ajouté." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("{id:int}/Membres/Importer")]
    [Authorize(Policy = "Droit:COHORTE.MODIFIER")]
    public async Task<IActionResult> ImporterMembres(int id)
    {
        var cohorte = await cohorteService.GetResumeAsync(id);
        if (cohorte is null)
        {
            return NotFound();
        }

        return View(new ImporterMembresFormModel { CohorteId = id, CohorteNom = cohorte.Nom });
    }

    [HttpPost("{id:int}/Membres/Importer")]
    [Authorize(Policy = "Droit:COHORTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImporterMembres(int id, ImporterMembresFormModel model)
    {
        var lignes = (model.Lignes ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var membres = new List<MembreImportInput>();
        foreach (var ligne in lignes)
        {
            var champs = ligne.Split(',', StringSplitOptions.TrimEntries);
            membres.Add(new MembreImportInput
            {
                Email = champs[0],
                Prenom = champs.Length > 1 ? champs[1] : null,
                Nom = champs.Length > 2 ? champs[2] : null,
            });
        }

        var gestionnaireId = userManager.GetUserId(User)!;
        var rapport = await cohorteService.ImporterMembresAsync(id, membres, gestionnaireId, token =>
            Url.Page("/Compte/DefinirMotDePasse", null, new { token }, Request.Scheme) ?? $"/Compte/DefinirMotDePasse?token={token}");

        TempData["StatusMessage"] = $"Import terminé : {rapport.ComptesCrees} compte(s) créé(s), {rapport.ComptesExistantsRattaches} compte(s) existant(s) rattaché(s), {rapport.DejaMembres} déjà membre(s)." +
            (rapport.Erreurs.Count > 0 ? $" {rapport.Erreurs.Count} erreur(s) : {string.Join(" | ", rapport.Erreurs)}" : "");

        return RedirectToAction(nameof(Details), new { id });
    }

    // ---- Demandes d'embarquement (prompt section H) ----

    [HttpGet("Embarquement")]
    [Authorize(Policy = "Droit:COHORTE.CONSULTER")]
    public async Task<IActionResult> Embarquement()
    {
        var demandes = await cohorteService.GetDemandesEmbarquementAsync();
        return View(demandes);
    }

    [HttpPost("Embarquement/{cohorteId:int}/Valider")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderEmbarquement(int cohorteId, string nom, DateTime dateLancement)
    {
        var lienFormations = Url.Action("Formations", "Home", null, Request.Scheme) ?? "/formations";
        var (success, errorMessage) = await cohorteService.ValiderEmbarquementAsync(cohorteId, nom, dateLancement, lienFormations);
        TempData["StatusMessage"] = success
            ? "Demande validée : la Cohorte est désormais ouverte à l'inscription publique."
            : errorMessage;

        return success ? RedirectToAction(nameof(Details), new { id = cohorteId }) : RedirectToAction(nameof(Embarquement));
    }

    [HttpPost("Embarquement/{cohorteId:int}/Refuser")]
    [Authorize(Policy = "Droit:COHORTE.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefuserEmbarquement(int cohorteId)
    {
        var lienCatalogue = Url.Action("Formations", "Home", null, Request.Scheme) ?? "/formations";
        var (success, errorMessage) = await cohorteService.RefuserEmbarquementAsync(cohorteId, lienCatalogue);
        TempData["StatusMessage"] = success ? "Demande refusée et demandeurs notifiés." : errorMessage;
        return RedirectToAction(nameof(Embarquement));
    }

    [HttpPost("Membres/{utilisateurId}/RenvoyerInvitation")]
    [Authorize(Policy = "Droit:COHORTE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenvoyerInvitation(string utilisateurId, int cohorteId)
    {
        var (success, errorMessage) = await cohorteService.RenvoyerInvitationAsync(utilisateurId, token =>
            Url.Page("/Compte/DefinirMotDePasse", null, new { token }, Request.Scheme) ?? $"/Compte/DefinirMotDePasse?token={token}");

        TempData["StatusMessage"] = success ? "Invitation renvoyée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id = cohorteId });
    }

    private async Task<List<SelectListItem>> ListeChallengesPublies()
    {
        var challenges = await challengeService.GetAllAsync();
        return challenges
            .Where(c => c.Statut == StatutChallenge.Publie)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = $"{c.Titre} ({c.Mode})" })
            .ToList();
    }

    private async Task<List<SelectListItem>> ListeOrganisations()
    {
        var organisations = await organisationService.GetAllAsync();
        return organisations
            .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.RaisonSociale })
            .ToList();
    }

    public sealed class CohorteFormModel
    {
        [Required(ErrorMessage = "Sélectionnez un Challenge publié.")]
        [Display(Name = "Challenge")]
        public int ChallengeId { get; set; }

        [Required(ErrorMessage = "Le nom de la Cohorte est obligatoire.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Display(Name = "Date de lancement (informative)")]
        [DataType(DataType.Date)]
        public DateTime? DateLancement { get; set; }

        [Display(Name = "Entreprise (BtoB uniquement)")]
        public int? OrganisationId { get; set; }

        public List<SelectListItem> Challenges { get; set; } = [];
        public List<SelectListItem> Organisations { get; set; } = [];
    }

    public sealed class ImporterMembresFormModel
    {
        public int CohorteId { get; set; }
        public string CohorteNom { get; set; } = string.Empty;

        [Display(Name = "Emails (un par ligne, format : email,prénom,nom)")]
        public string? Lignes { get; set; }
    }

    public sealed class LancerFormModel
    {
        public int CohorteId { get; set; }
        public string CohorteNom { get; set; } = string.Empty;
        public string ChallengeTitre { get; set; } = string.Empty;
        public string TitreEtape { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date et l'heure de la visio sont obligatoires.")]
        [Display(Name = "Date et heure de la visio")]
        public DateTime? DateHeureVisio { get; set; }

        [Required(ErrorMessage = "Le lien de connexion de la visio est obligatoire.")]
        [Display(Name = "Lien de connexion")]
        public string LienConnexionVisio { get; set; } = string.Empty;

        [Display(Name = "Descriptif / agenda de la visio")]
        public string DescriptifVisio { get; set; } = string.Empty;
    }

    public sealed class ValiderEtapeFormModel
    {
        public int CohorteId { get; set; }
        public string CohorteNom { get; set; } = string.Empty;
        public string ChallengeTitre { get; set; } = string.Empty;
        public int NumeroEtapeSuivante { get; set; }
        public string TitreEtapeSuivante { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date et l'heure de la visio sont obligatoires.")]
        [Display(Name = "Date et heure de la visio")]
        public DateTime? DateHeureVisio { get; set; }

        [Required(ErrorMessage = "Le lien de connexion de la visio est obligatoire.")]
        [Display(Name = "Lien de connexion")]
        public string LienConnexionVisio { get; set; } = string.Empty;

        [Display(Name = "Descriptif / agenda de la visio")]
        public string DescriptifVisio { get; set; } = string.Empty;
    }
}
