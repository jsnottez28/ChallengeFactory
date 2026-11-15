using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

// Back-office de la vitrine "Catalogue de formations" (/catalogue-formations) - cf.
// OffreFormation pour le mecanisme commun entre formations deja montees et formations
// encore a l'etat de catalogue marketing.
[Route("Administration/[controller]")]
public class OffresFormationController(IOffreFormationService offreFormationService, IChallengeService challengeService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:OFFREFORMATION.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var offres = await offreFormationService.GetAllAsync();
        return View(offres);
    }

    [HttpGet("Create")]
    [Authorize(Policy = "Droit:OFFREFORMATION.CREER")]
    public async Task<IActionResult> Create()
    {
        await ChargerChallengesAsync();
        return View("Save", new OffreFormationFormModel());
    }

    [HttpPost("Create")]
    [Authorize(Policy = "Droit:OFFREFORMATION.CREER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OffreFormationFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await ChargerChallengesAsync();
            return View("Save", model);
        }

        var (success, errorMessage, offre) = await offreFormationService.CreateAsync(VersInput(model));
        if (!success)
        {
            ModelState.AddModelError(nameof(model.Titre), errorMessage ?? "Impossible de créer cette formation.");
            await ChargerChallengesAsync();
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Formation créée.";
        return RedirectToAction(nameof(Edit), new { id = offre!.Id });
    }

    [HttpGet("Edit/{id:int}")]
    [Authorize(Policy = "Droit:OFFREFORMATION.MODIFIER")]
    public async Task<IActionResult> Edit(int id)
    {
        var offre = await offreFormationService.GetByIdAsync(id);
        if (offre is null)
        {
            return NotFound();
        }

        await ChargerChallengesAsync();
        return View("Save", new OffreFormationFormModel
        {
            Id = offre.Id,
            Titre = offre.Titre,
            Categorie = offre.Categorie,
            LienAvecOffre = offre.LienAvecOffre,
            Duree = offre.Duree,
            Public = offre.Public,
            Financement = offre.Financement,
            Prix = offre.Prix,
            DisponibleEnChallenge = offre.DisponibleEnChallenge,
            DisponibleEnPresentiel = offre.DisponibleEnPresentiel,
            Cible = offre.Cible,
            Statut = offre.Statut,
            ChallengeId = offre.ChallengeId,
            LienPersonnalise = offre.LienPersonnalise,
            Ordre = offre.Ordre,
            EstPublie = offre.EstPublie,
        });
    }

    [HttpPost("Edit/{id:int}")]
    [Authorize(Policy = "Droit:OFFREFORMATION.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OffreFormationFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await ChargerChallengesAsync();
            return View("Save", model);
        }

        var (success, errorMessage) = await offreFormationService.UpdateAsync(id, VersInput(model));
        if (!success)
        {
            ModelState.AddModelError(nameof(model.Titre), errorMessage ?? "Impossible de modifier cette formation.");
            await ChargerChallengesAsync();
            return View("Save", model);
        }

        TempData["StatusMessage"] = "Formation modifiée.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/Supprimer")]
    [Authorize(Policy = "Droit:OFFREFORMATION.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await offreFormationService.DeleteAsync(id);
        TempData["StatusMessage"] = success ? "Formation supprimée." : errorMessage;
        return RedirectToAction(nameof(Index));
    }

    private async Task ChargerChallengesAsync()
    {
        var challenges = await challengeService.GetAllAsync();
        ViewData["Challenges"] = challenges.OrderBy(c => c.Titre).ToList();
    }

    private static OffreFormationInput VersInput(OffreFormationFormModel model) => new()
    {
        Titre = model.Titre,
        Categorie = model.Categorie,
        LienAvecOffre = model.LienAvecOffre,
        Duree = model.Duree,
        Public = model.Public,
        Financement = model.Financement,
        Prix = model.Prix,
        DisponibleEnChallenge = model.DisponibleEnChallenge,
        DisponibleEnPresentiel = model.DisponibleEnPresentiel,
        Cible = model.Cible,
        Statut = model.Statut,
        ChallengeId = model.ChallengeId,
        LienPersonnalise = model.LienPersonnalise,
        Ordre = model.Ordre,
        EstPublie = model.EstPublie,
    };

    public sealed class OffreFormationFormModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire.")]
        [Display(Name = "Titre")]
        public string Titre { get; set; } = string.Empty;

        [Display(Name = "Catégorie")]
        public CategorieOffreFormation Categorie { get; set; }

        [Display(Name = "Lien avec l'offre (phrase courte, optionnelle)")]
        public string? LienAvecOffre { get; set; }

        [Required(ErrorMessage = "La durée est obligatoire.")]
        [Display(Name = "Durée")]
        public string Duree { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le public visé est obligatoire.")]
        [Display(Name = "Public")]
        public string Public { get; set; } = string.Empty;

        [Display(Name = "Financement (OPCO, CPF, Gratuit...)")]
        public string? Financement { get; set; }

        [Display(Name = "Prix affiché")]
        public string Prix { get; set; } = "Sur devis";

        [Display(Name = "Disponible en format Challenge")]
        public bool DisponibleEnChallenge { get; set; } = true;

        [Display(Name = "Disponible en présentiel")]
        public bool DisponibleEnPresentiel { get; set; }

        [Display(Name = "Cible")]
        public ModePlateforme Cible { get; set; } = ModePlateforme.BtoB;

        [Display(Name = "Statut")]
        public StatutOffreFormation Statut { get; set; } = StatutOffreFormation.SurDemande;

        [Display(Name = "Challenge réel associé (une fois construit)")]
        public int? ChallengeId { get; set; }

        [Display(Name = "Lien personnalisé (si pas de Challenge, ex. page statique existante)")]
        public string? LienPersonnalise { get; set; }

        [Display(Name = "Ordre d'affichage dans la catégorie")]
        public int Ordre { get; set; }

        [Display(Name = "Publiée sur le site")]
        public bool EstPublie { get; set; } = true;
    }
}
