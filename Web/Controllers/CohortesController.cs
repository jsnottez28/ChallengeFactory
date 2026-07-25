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

        return View(cohorte);
    }

    [HttpPost("{id:int}/Lancer")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lancer(int id)
    {
        var lienMonParcours = Url.Page("/Dashboard/MonParcours", null, null, Request.Scheme) ?? "/Dashboard/MonParcours";
        var (success, errorMessage) = await cohorteService.LancerAsync(id, userManager.GetUserId(User)!, lienMonParcours);
        TempData["StatusMessage"] = success ? "Cohorte lancée : étape 1 attribuée et membres notifiés." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/ValiderEtape")]
    [Authorize(Policy = "Droit:COHORTE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValiderEtape(int id)
    {
        var lienMonParcours = Url.Page("/Dashboard/MonParcours", null, null, Request.Scheme) ?? "/Dashboard/MonParcours";
        var lienBibliotheque = Url.Page("/Dashboard/Cartes", null, null, Request.Scheme) ?? "/Dashboard/Cartes";

        var (success, errorMessage) = await cohorteService.ValiderEtapeAsync(id, userManager.GetUserId(User)!, lienMonParcours, lienBibliotheque);
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
}
