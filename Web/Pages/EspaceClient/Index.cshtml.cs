using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.EspaceClient;

// Accueil de l'espace client Bilan Carbone(R) : etat du dossier, acces au questionnaire de
// collecte, check-list des documents a rassembler une fois le questionnaire envoye. Les
// plans d'action de decarbonation viendront s'ajouter ici. Accessible a tout compte
// disposant d'un DossierCarbone (Participant ou ClientCarbone - les acces se cumulent,
// cf. Domain.Entities.TypeCompte), jamais filtre par TypeCompte seul.
[Authorize]
public class IndexModel(ICollecteCarboneService collecteService, UserManager<ApplicationUser> userManager) : PageModel
{
    private const long TailleMaxTableurOctets = 10 * 1024 * 1024; // 10 Mo

    public DossierCarboneInfo? Dossier { get; private set; }
    public List<ElementCollecteInfo> Checklist { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        Dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (Dossier is null)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        if (Dossier?.StatutQuestionnaire == StatutQuestionnaireCollecte.Soumis)
        {
            Checklist = await collecteService.GetChecklistAsync(Dossier.Id);
        }

        return Page();
    }

    // Self-service : le client telecharge directement son tableur de collecte, sans
    // attendre que le Gestionnaire le lui transmette manuellement. L'Id du Pre-diagnostic
    // vient toujours du dossier retrouve via l'utilisateur connecte, jamais d'une valeur
    // fournie par le client - meme principe que le reste de l'Espace Client.
    public async Task<IActionResult> OnGetTableurAsync()
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        var dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (dossier is null)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        var lienEspaceClient = Url.Page("/EspaceClient/Index", null, null, Request.Scheme);
        var tableur = await collecteService.GenererTableurAsync(dossier.PreBilanCarboneId, lienEspaceClient);
        if (tableur is null)
        {
            return NotFound();
        }

        return File(tableur.Value.Contenu, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tableur.Value.NomFichier);
    }

    // Self-service : le client depose son tableur rempli (remplace le depot precedent -
    // un seul conserve). La "jauge" affichee ensuite est recalculee a partir de CE
    // fichier, pas saisie a la main par le client.
    public async Task<IActionResult> OnPostDeposerTableurAsync(IFormFile? fichier, bool collecteTerminee)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        if (fichier is null || fichier.Length == 0)
        {
            TempData["ErreurDepotTableur"] = "Merci de choisir un fichier avant de le déposer.";
            return RedirectToPage();
        }

        if (!string.Equals(Path.GetExtension(fichier.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErreurDepotTableur"] = "Seuls les fichiers .xlsx sont acceptés.";
            return RedirectToPage();
        }

        if (fichier.Length > TailleMaxTableurOctets)
        {
            TempData["ErreurDepotTableur"] = $"Ce fichier dépasse la taille maximale autorisée ({TailleMaxTableurOctets / (1024 * 1024)} Mo).";
            return RedirectToPage();
        }

        await using var flux = fichier.OpenReadStream();
        var (success, errorMessage) = await collecteService.DeposerTableurRempliAsync(utilisateur.Id, flux, fichier.FileName, collecteTerminee);

        TempData[success ? "StatusMessage" : "ErreurDepotTableur"] = success
            ? "Votre tableur a bien été déposé. L'avancement estimé a été mis à jour."
            : errorMessage;

        return RedirectToPage();
    }
}
