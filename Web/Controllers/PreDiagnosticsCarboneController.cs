using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

// Cote Gestionnaire (CRM) : liste des Pre-diagnostics carbone deposes par des prospects -
// cf. Web.Controllers.PreDiagnosticCarboneController pour le formulaire public.
[Route("Administration/[controller]")]
public class PreDiagnosticsCarboneController(
    IPreBilanCarboneService preBilanService,
    ICollecteCarboneService collecteService,
    UserManager<ApplicationUser> userManager) : Controller
{
    private const long TailleMaxTableurOctets = 10 * 1024 * 1024; // 10 Mo


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
        var (success, errorMessage) = await collecteService.OuvrirCompteClientAsync(id, gestionnaireId, LienActivation, LienConnexion());
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

    [HttpPost("{id:int}/RenvoyerEmailOuverture")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenvoyerEmailOuverture(int id)
    {
        var (success, errorMessage) = await collecteService.RenvoyerEmailOuvertureAsync(id, LienActivation, LienConnexion());
        TempData["StatusMessage"] = success ? "Email d'ouverture d'espace renvoyé au client." : errorMessage;
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

    // Saisie par le consultant pour le compte du client (donnees recueillies par un autre
    // canal - visite terrain, appel...), mode simple : meme formulaire web que cote client.
    [HttpPost("{id:int}/EnregistrerValeurs")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnregistrerValeursGestionnaire(int id, List<ValeurSaisieLigneForm>? lignes)
    {
        var gestionnaire = await userManager.GetUserAsync(User);
        var nomAffiche = gestionnaire?.Prenom ?? gestionnaire?.Email ?? "Le consultant";

        var donnees = (lignes ?? []).Select(l => (l.Cle, new ValeurSaisieFichier
        {
            LibellePersonnalise = l.LibellePersonnalise,
            Valeur = l.Valeur,
            Unite = l.Unite,
            Type = l.Type,
            Source = l.Source,
            Hypotheses = l.Hypotheses,
            Statut = l.Statut,
            Commentaire = l.Commentaire,
        })).ToList();

        var (success, errorMessage) = await collecteService.EnregistrerValeursFormulaireParGestionnaireAsync(id, nomAffiche, donnees);
        TempData["StatusMessage"] = success ? "Données enregistrées." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    // Saisie par le consultant pour le compte du client, mode avance : depot d'un tableur
    // rempli par le consultant lui-meme (reunion sur site, fichier recu par email...).
    [HttpPost("{id:int}/DeposerTableur")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeposerTableurGestionnaire(int id, IFormFile? fichier)
    {
        if (fichier is null || fichier.Length == 0)
        {
            TempData["StatusMessage"] = "Merci de choisir un fichier avant de le déposer.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!string.Equals(Path.GetExtension(fichier.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["StatusMessage"] = "Seuls les fichiers .xlsx sont acceptés.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (fichier.Length > TailleMaxTableurOctets)
        {
            TempData["StatusMessage"] = $"Ce fichier dépasse la taille maximale autorisée ({TailleMaxTableurOctets / (1024 * 1024)} Mo).";
            return RedirectToAction(nameof(Details), new { id });
        }

        var gestionnaire = await userManager.GetUserAsync(User);
        var nomAffiche = gestionnaire?.Prenom ?? gestionnaire?.Email ?? "Le consultant";

        await using var flux = fichier.OpenReadStream();
        var (success, errorMessage) = await collecteService.DeposerTableurRempliParGestionnaireAsync(id, flux, fichier.FileName, nomAffiche);
        TempData["StatusMessage"] = success ? "Tableur déposé : l'avancement a été mis à jour." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/EnregistrerLigneLibre")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnregistrerLigneLibreGestionnaire(int id, LigneLibreForm ligne)
    {
        var gestionnaire = await userManager.GetUserAsync(User);
        var nomAffiche = gestionnaire?.Prenom ?? gestionnaire?.Email ?? "Le consultant";

        var (success, errorMessage) = await collecteService.EnregistrerLigneLibreParGestionnaireAsync(id, nomAffiche, new LigneLibreCollecte
        {
            Id = ligne.Id ?? string.Empty,
            Poste = ligne.Poste,
            Donnee = ligne.Donnee,
            Valeur = ligne.Valeur,
            Unite = ligne.Unite,
            Type = ligne.Type,
            Source = ligne.Source,
            Hypotheses = ligne.Hypotheses,
            Statut = ligne.Statut,
            Commentaire = ligne.Commentaire,
        });
        TempData["StatusMessage"] = success ? "Ligne enregistrée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/SupprimerLigneLibre")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SupprimerLigneLibreGestionnaire(int id, string ligneId)
    {
        var (success, errorMessage) = await collecteService.SupprimerLigneLibreParGestionnaireAsync(id, ligneId);
        TempData["StatusMessage"] = success ? "Ligne supprimée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/InviterCollaborateur")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviterCollaborateur(int id, string email)
    {
        var dossier = await collecteService.GetDossierParPreBilanAsync(id);
        if (dossier is null)
        {
            TempData["StatusMessage"] = "Ouvrez d'abord l'espace client.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var gestionnaireId = userManager.GetUserId(User) ?? string.Empty;
        var (success, errorMessage) = await collecteService.InviterCollaborateurAsync(dossier.Id, email, gestionnaireId, LienActivation, LienConnexion());
        TempData["StatusMessage"] = success ? "Invitation envoyée." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/RetirerCollaborateur")]
    [Authorize(Policy = "Droit:PREBILAN.MODIFIER")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetirerCollaborateur(int id, string utilisateurId)
    {
        var dossier = await collecteService.GetDossierParPreBilanAsync(id);
        if (dossier is null)
        {
            TempData["StatusMessage"] = "Ouvrez d'abord l'espace client.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var (success, errorMessage) = await collecteService.RetirerCollaborateurAsync(dossier.Id, utilisateurId);
        TempData["StatusMessage"] = success ? "Accès retiré." : errorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    private string LienActivation(string token) =>
        Url.Page("/Compte/DefinirMotDePasse", null, new { token }, Request.Scheme) ?? $"/Compte/DefinirMotDePasse?token={token}";

    private string LienConnexion() =>
        Url.Page("/Account/Login", null, new { area = "Identity" }, Request.Scheme) ?? "/Identity/Account/Login";
}
