using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers;

// Cote Gestionnaire (CRM) : liste des Pre-diagnostics carbone deposes par des prospects -
// cf. Web.Controllers.PreDiagnosticCarboneController pour le formulaire public.
[Route("Administration/[controller]")]
public class PreDiagnosticsCarboneController(
    IPreBilanCarboneService preBilanService,
    ICollecteCarboneService collecteService,
    UserManager<ApplicationUser> userManager) : Controller
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

        var dossier = await collecteService.GetDossierParPreBilanAsync(id);
        ViewData["Dossier"] = dossier;
        if (dossier?.StatutQuestionnaire == StatutQuestionnaireCollecte.Soumis)
        {
            ViewData["Checklist"] = await collecteService.GetChecklistAsync(dossier.Id);
        }

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

    // --- Dossier client Bilan Carbone(R) (cf. ICollecteCarboneService) ---------------

    [HttpPost("{id:int}/OuvrirCompteClient")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OuvrirCompteClient(int id)
    {
        var gestionnaireId = userManager.GetUserId(User) ?? string.Empty;
        var (success, errorMessage) = await collecteService.OuvrirCompteClientAsync(id, gestionnaireId, LienActivation);
        TempData["StatusMessage"] = success
            ? "Espace client ouvert : le client a reçu son identifiant et le lien pour définir son mot de passe."
            : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/RenvoyerInvitationClient")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenvoyerInvitationClient(int id)
    {
        var (success, errorMessage) = await collecteService.RenvoyerInvitationAsync(id, LienActivation);
        TempData["StatusMessage"] = success ? "Nouvelle invitation envoyée au client." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/EnvoyerQuestionnaire")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnvoyerQuestionnaire(int id)
    {
        var lien = Url.Page("/EspaceClient/Questionnaire", null, null, Request.Scheme) ?? "/EspaceClient/Questionnaire";
        var (success, errorMessage) = await collecteService.EnvoyerQuestionnaireAsync(id, lien);
        TempData["StatusMessage"] = success ? "Questionnaire envoyé : le client a reçu le lien vers son espace." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet("{id:int}/TableurCollecte")]
    [Authorize(Policy = "Droit:PREBILAN.CONSULTER")]
    public async Task<IActionResult> TableurCollecte(int id)
    {
        var lienEspaceClient = Url.Page("/EspaceClient/Index", null, null, Request.Scheme);
        var tableur = await collecteService.GenererTableurAsync(id, lienEspaceClient);
        if (tableur is null)
        {
            return NotFound();
        }

        return File(tableur.Value.Contenu, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tableur.Value.NomFichier);
    }

    // Le dernier tableur depose PAR LE CLIENT (fichier tel quel, pas regenere) - distinct
    // de TableurCollecte ci-dessus qui genere toujours la version pre-remplie a jour.
    [HttpGet("{id:int}/TableurDepose")]
    [Authorize(Policy = "Droit:PREBILAN.CONSULTER")]
    public async Task<IActionResult> TableurDepose(int id)
    {
        var tableur = await collecteService.TelechargerDernierTableurDeposeAsync(id);
        if (tableur is null)
        {
            return NotFound();
        }

        return File(tableur.Value.Contenu, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tableur.Value.NomFichier);
    }

    private string LienActivation(string token) =>
        Url.Page("/Compte/DefinirMotDePasse", null, new { token }, Request.Scheme) ?? $"/Compte/DefinirMotDePasse?token={token}";
}
