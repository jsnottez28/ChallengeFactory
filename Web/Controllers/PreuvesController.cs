using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers;

// Vues Gestionnaire (droits PREUVE.*/FORUM.SUPPRIMER) - jamais exposees a un apprenant,
// meme indirectement (cf. prompt section 5, regle d'or : aucun classement individuel
// de performance ne doit etre expose a un apprenant).
[Route("Administration/Preuves")]
public class PreuvesController(
    IPreuveService preuveService,
    IForumService forumService,
    ICohorteService cohorteService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("Etape/{cohorteId:int}")]
    [Authorize(Policy = "Droit:PREUVE.CONSULTER")]
    public async Task<IActionResult> Etape(int cohorteId)
    {
        var cohorte = await cohorteService.GetResumeAsync(cohorteId);
        if (cohorte is null)
        {
            return NotFound();
        }

        ViewData["Cohorte"] = cohorte;
        ViewData["Resume"] = await preuveService.GetResumeAvantClotureAsync(cohorteId);

        var preuves = await preuveService.GetPreuvesEtapeAsync(cohorteId);
        return View(preuves);
    }

    [HttpPost("{preuveId:int}/Valider")]
    [Authorize(Policy = "Droit:PREUVE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Valider(int preuveId, int cohorteId, string? commentaire)
    {
        var valideurId = userManager.GetUserId(User)!;
        var (success, errorMessage) = await preuveService.ValiderParGestionnaireAsync(
            preuveId, valideurId, DecisionValidationGestionnaire.Valide, commentaire);

        TempData["StatusMessage"] = success ? "Preuve validée." : errorMessage;
        return RedirectToAction(nameof(Etape), new { cohorteId });
    }

    [HttpPost("{preuveId:int}/Refuser")]
    [Authorize(Policy = "Droit:PREUVE.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refuser(int preuveId, int cohorteId, string commentaire)
    {
        var valideurId = userManager.GetUserId(User)!;
        var (success, errorMessage) = await preuveService.ValiderParGestionnaireAsync(
            preuveId, valideurId, DecisionValidationGestionnaire.Refuse, commentaire);

        TempData["StatusMessage"] = success ? "Preuve refusée." : errorMessage;
        return RedirectToAction(nameof(Etape), new { cohorteId });
    }

    [HttpGet("Contributions/{cohorteId:int}")]
    [Authorize(Policy = "Droit:PREUVE.CONSULTER")]
    public async Task<IActionResult> Contributions(int cohorteId)
    {
        var cohorte = await cohorteService.GetResumeAsync(cohorteId);
        if (cohorte is null)
        {
            return NotFound();
        }

        ViewData["Cohorte"] = cohorte;
        var contributions = await preuveService.GetContributionsCohorteAsync(cohorteId);
        return View(contributions);
    }

    [HttpGet("Forum/{cohorteId:int}")]
    [Authorize(Policy = "Droit:FORUM.CONSULTER")]
    public async Task<IActionResult> Forum(int cohorteId, int? challengeEtapeId)
    {
        var cohorte = await cohorteService.GetResumeAsync(cohorteId);
        if (cohorte is null)
        {
            return NotFound();
        }

        var utilisateurId = userManager.GetUserId(User)!;
        var etapes = await forumService.GetEtapesAccessiblesAsync(cohorteId, utilisateurId, estAdmin: true);
        var etapeAffichee = challengeEtapeId is int id
            ? etapes.FirstOrDefault(e => e.ChallengeEtapeId == id)
            : etapes.FirstOrDefault(e => e.EstEtapeCourante) ?? etapes.LastOrDefault();

        ViewData["Cohorte"] = cohorte;
        ViewData["Etapes"] = etapes;
        ViewData["EtapeAffichee"] = etapeAffichee;

        var messages = etapeAffichee is null
            ? []
            : await forumService.GetMessagesEtapeAsync(etapeAffichee.ChallengeEtapeId, cohorteId, utilisateurId, estAdmin: true);

        return View(messages);
    }

    [HttpPost("Forum/{messageId:int}/Supprimer")]
    [Authorize(Policy = "Droit:FORUM.SUPPRIMER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerMessage(int messageId, int cohorteId, int? challengeEtapeId)
    {
        var (success, errorMessage) = await forumService.SupprimerMessageAsync(messageId);
        TempData["StatusMessage"] = success ? "Message supprimé." : errorMessage;
        return RedirectToAction(nameof(Forum), new { cohorteId, challengeEtapeId });
    }
}
