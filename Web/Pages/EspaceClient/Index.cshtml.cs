using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.EspaceClient;

// Accueil de l'espace client Bilan Carbone(R) (comptes TypeCompte.ClientCarbone
// uniquement) : etat du dossier, acces au questionnaire de collecte, check-list des
// documents a rassembler une fois le questionnaire envoye. Les plans d'action de
// decarbonation viendront s'ajouter ici.
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

        if (utilisateur.TypeCompte != TypeCompte.ClientCarbone)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        Dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (Dossier?.StatutQuestionnaire == StatutQuestionnaireCollecte.Soumis)
        {
            Checklist = await collecteService.GetChecklistAsync(Dossier.Id);
        }

        return Page();
    }
}
