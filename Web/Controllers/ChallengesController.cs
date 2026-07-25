using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class ChallengesController(IChallengeService challengeService, ICarteCompetenceService carteCompetenceService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:CHALLENGE.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var challenges = await challengeService.GetAllAsync();
        return View(challenges);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:CHALLENGE.CREER")]
    public IActionResult Create()
    {
        return View("Save", new ChallengeFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:CHALLENGE.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChallengeFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var (success, errorMessage, challenge) = await challengeService.CreateAsync(VersInput(model));

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Titre), errorMessage ?? "Impossible de créer ce Challenge.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Challenge créé. Ajoutez maintenant ses étapes.";
        return RedirectToAction(nameof(Details), new { id = challenge!.Id });
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var challenge = await challengeService.GetByIdAsync(id);
        if (challenge is null)
        {
            return NotFound();
        }

        return View("Save", new ChallengeFormModel
        {
            Id = challenge.Id,
            Titre = challenge.Titre,
            Slogan = challenge.Slogan,
            NombreEtapes = challenge.NombreEtapes,
            Mode = challenge.Mode,
        });
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ChallengeFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Save", model);
        }

        var (success, errorMessage) = await challengeService.UpdateAsync(id, VersInput(model));

        if (!success)
        {
            ModelState.AddModelError(nameof(model.Titre), errorMessage ?? "Impossible de modifier ce Challenge.");
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Challenge modifié.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("Details/{id:int}")]
    [Authorize(Policy = "Droit:CHALLENGE.CONSULTER")]
    public async Task<IActionResult> Details(int id)
    {
        var challenge = await challengeService.GetByIdAsync(id);
        if (challenge is null)
        {
            return NotFound();
        }

        return View(challenge);
    }

    [HttpPost("{id:int}/Publier")]
    [Authorize(Policy = "Droit:CHALLENGE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publier(int id)
    {
        var (success, errorMessage) = await challengeService.PublierAsync(id);
        TempData["StatusMessage"] = success ? "Challenge publié." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("{challengeId:int}/Etapes/Create")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    public IActionResult CreateEtape(int challengeId)
    {
        return View("SaveEtape", new ChallengeEtapeFormModel { ChallengeId = challengeId });
    }

    [HttpPost("{challengeId:int}/Etapes/Create")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEtape(int challengeId, ChallengeEtapeFormModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ChallengeId = challengeId;
            return View("SaveEtape", model);
        }

        var (success, errorMessage, _) = await challengeService.CreerEtapeAsync(challengeId, VersEtapeInput(model));

        if (!success)
        {
            ModelState.AddModelError(nameof(model.TitreEtape), errorMessage ?? "Impossible de créer cette étape.");
            model.ChallengeId = challengeId;
            return View("SaveEtape", model);
        }

        TempData["StatusMessage"] = "Étape créée.";
        return RedirectToAction(nameof(Details), new { id = challengeId });
    }

    [HttpGet("Etapes/{etapeId:int}/Edit")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    public async Task<IActionResult> EditEtape(int etapeId)
    {
        var etape = await challengeService.GetEtapeByIdAsync(etapeId);
        if (etape is null)
        {
            return NotFound();
        }

        return View("SaveEtape", new ChallengeEtapeFormModel
        {
            Id = etape.Id,
            ChallengeId = etape.ChallengeId,
            TitreEtape = etape.TitreEtape,
            ObjectifPedagogique = etape.ObjectifPedagogique,
            CompetenceCible = etape.CompetenceCible,
            DefiIndividuel = etape.DefiIndividuel,
        });
    }

    [HttpPost("Etapes/{etapeId:int}/Edit")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEtape(int etapeId, ChallengeEtapeFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("SaveEtape", model);
        }

        var (success, errorMessage) = await challengeService.ModifierEtapeAsync(etapeId, VersEtapeInput(model));

        if (!success)
        {
            ModelState.AddModelError(nameof(model.TitreEtape), errorMessage ?? "Impossible de modifier cette étape.");
            return View("SaveEtape", model);
        }

        TempData["StatusMessage"] = "Étape modifiée.";
        return RedirectToAction(nameof(Details), new { id = model.ChallengeId });
    }

    [HttpPost("Etapes/{etapeId:int}/Delete")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEtape(int etapeId, int challengeId)
    {
        var (success, errorMessage) = await challengeService.SupprimerEtapeAsync(etapeId);
        TempData["StatusMessage"] = success ? "Étape supprimée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id = challengeId });
    }

    [HttpGet("Etapes/{etapeId:int}/Cartes")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    public async Task<IActionResult> CartesEtape(int etapeId, string? recherche)
    {
        var etape = await challengeService.GetEtapeByIdAsync(etapeId);
        if (etape is null)
        {
            return NotFound();
        }

        var resultat = await carteCompetenceService.RechercherAsync(new CarteCompetenceFiltre { Recherche = recherche, TaillePage = 500 });
        var idsSelectionnes = etape.Cartes.Select(c => c.CarteCompetenceId).ToHashSet();

        ViewData["ChallengeId"] = etape.ChallengeId;
        ViewData["EtapeId"] = etapeId;
        ViewData["EtapeTitre"] = etape.TitreEtape;
        ViewData["Recherche"] = recherche;
        ViewData["IdsSelectionnes"] = idsSelectionnes;

        return View(resultat.Cartes);
    }

    [HttpPost("Etapes/{etapeId:int}/Cartes")]
    [Authorize(Policy = "Droit:CHALLENGE.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CartesEtape(int etapeId, int challengeId, [FromForm] List<int>? carteIds)
    {
        var (success, errorMessage) = await challengeService.DefinirCartesEtapeAsync(etapeId, carteIds ?? []);
        TempData["StatusMessage"] = success ? "Ressources Directrices mises à jour." : errorMessage;
        return RedirectToAction(nameof(Details), new { id = challengeId });
    }

    private static ChallengeInput VersInput(ChallengeFormModel model) => new()
    {
        Titre = model.Titre,
        Slogan = model.Slogan,
        NombreEtapes = model.NombreEtapes,
        Mode = model.Mode,
    };

    private static ChallengeEtapeInput VersEtapeInput(ChallengeEtapeFormModel model) => new()
    {
        TitreEtape = model.TitreEtape,
        ObjectifPedagogique = model.ObjectifPedagogique,
        CompetenceCible = model.CompetenceCible,
        DefiIndividuel = model.DefiIndividuel,
    };

    public sealed class ChallengeFormModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire.")]
        [Display(Name = "Titre")]
        public string Titre { get; set; } = string.Empty;

        [Display(Name = "Slogan")]
        public string? Slogan { get; set; }

        [Range(1, 52, ErrorMessage = "Le nombre d'étapes doit être compris entre 1 et 52.")]
        [Display(Name = "Nombre d'étapes")]
        public int NombreEtapes { get; set; } = 8;

        [Display(Name = "Mode")]
        public ModePlateforme Mode { get; set; }
    }

    public sealed class ChallengeEtapeFormModel
    {
        public int? Id { get; set; }
        public int ChallengeId { get; set; }

        [Required(ErrorMessage = "Le titre de l'étape est obligatoire.")]
        [Display(Name = "Titre de l'étape")]
        public string TitreEtape { get; set; } = string.Empty;

        [Display(Name = "Objectif pédagogique")]
        public string? ObjectifPedagogique { get; set; }

        [Display(Name = "Compétence cible")]
        public string? CompetenceCible { get; set; }

        [Display(Name = "Défi individuel")]
        public string? DefiIndividuel { get; set; }
    }
}
