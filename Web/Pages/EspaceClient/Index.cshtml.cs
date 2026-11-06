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

        var tableur = await collecteService.GenererTableurAsync(dossier.PreBilanCarboneId);
        if (tableur is null)
        {
            return NotFound();
        }

        return File(tableur.Value.Contenu, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tableur.Value.NomFichier);
    }
}
