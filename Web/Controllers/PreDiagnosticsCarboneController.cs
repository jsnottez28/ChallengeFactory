using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

// Cote Gestionnaire (CRM) : liste des Pre-diagnostics carbone deposes par des prospects -
// cf. Web.Controllers.PreDiagnosticCarboneController pour le formulaire public.
[Route("Administration/[controller]")]
public class PreDiagnosticsCarboneController(IPreBilanCarboneService preBilanService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = "Droit:PREBILAN.CONSULTER")]
    public async Task<IActionResult> Index()
    {
        var preBilans = await preBilanService.GetTousAsync();
        return View(preBilans);
    }

    [HttpGet("Details/{id:int}")]
    [Authorize(Policy = "Droit:PREBILAN.CONSULTER")]
    public async Task<IActionResult> Details(int id)
    {
        var resultat = await preBilanService.GetResultatAsync(id);
        if (resultat is null)
        {
            return NotFound();
        }

        var tous = await preBilanService.GetTousAsync();
        var infoCrm = tous.FirstOrDefault(p => p.Id == id);
        if (infoCrm is null)
        {
            return NotFound();
        }

        ViewData["InfoCrm"] = infoCrm;
        return View(resultat);
    }

    [HttpPost("{id:int}/Statut")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerStatut(int id, StatutPreBilanCrm statut)
    {
        var (success, errorMessage) = await preBilanService.ChangerStatutCrmAsync(id, statut);
        TempData["StatusMessage"] = success ? "Statut mis à jour." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }
}
