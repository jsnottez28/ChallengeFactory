using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers;

[Route("Administration/[controller]")]
public class ReclamationsController(
    IReclamationService reclamationService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:RECLAMATION.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var reclamations = await reclamationService.GetAllAsync();
        return View(reclamations);
    }

    [HttpGet("Details/{id:int}")]
    [Authorize(Policy = "Droit:RECLAMATION.CONSULTER")]
    public async Task<IActionResult> Details(int id)
    {
        var reclamation = await reclamationService.GetByIdAsync(id);
        if (reclamation is null)
        {
            return NotFound();
        }

        return View(reclamation);
    }

    [HttpPost("{id:int}/PrendreEnCharge")]
    [Authorize(Policy = "Droit:RECLAMATION.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrendreEnCharge(int id)
    {
        var (success, errorMessage) = await reclamationService.PrendreEnChargeAsync(id, userManager.GetUserId(User)!);
        TempData["StatusMessage"] = success ? "Réclamation prise en charge." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/Repondre")]
    [Authorize(Policy = "Droit:RECLAMATION.VALIDER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Repondre(int id, string reponse)
    {
        var (success, errorMessage) = await reclamationService.RepondreAsync(id, userManager.GetUserId(User)!, reponse);
        TempData["StatusMessage"] = success ? "Réponse envoyée à l'auteur de la réclamation." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }
}
