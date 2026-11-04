using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.EspaceClient;

// Questionnaire de collecte (questions de filtrage du Bilan Carbone(R)), rempli par le
// compte ClientCarbone depuis le lien recu par email. Le dossier est toujours retrouve a
// partir de l'utilisateur connecte, jamais d'un Id passe dans l'URL.
[Authorize]
public class QuestionnaireModel(ICollecteCarboneService collecteService, UserManager<ApplicationUser> userManager) : PageModel
{
    public DossierCarboneInfo? Dossier { get; private set; }

    // Cle = code de question (QuestionnaireCollecteCatalogue), lu depuis
    // Request.Form["Reponses[CODE]"] via le binder par defaut.
    [BindProperty]
    public Dictionary<string, string> Reponses { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var (_, refus) = await ChargerAsync();
        if (refus is not null)
        {
            return refus;
        }

        Reponses = Dossier!.Reponses;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? bouton)
    {
        var (utilisateur, refus) = await ChargerAsync();
        if (refus is not null)
        {
            return refus;
        }

        var soumettre = bouton == "envoyer";
        var (success, errorMessage) = await collecteService.EnregistrerReponsesAsync(utilisateur!.Id, Reponses, soumettre);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Une erreur est survenue.");
            return Page();
        }

        if (soumettre)
        {
            TempData["StatusMessage"] = "Merci ! Votre questionnaire a bien été envoyé. Voici la liste des documents à rassembler.";
            return RedirectToPage("/EspaceClient/Index");
        }

        TempData["StatusMessage"] = "Vos réponses sont enregistrées. Vous pouvez reprendre plus tard.";
        return RedirectToPage();
    }

    private async Task<(ApplicationUser? Utilisateur, IActionResult? Refus)> ChargerAsync()
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return (null, Challenge());
        }

        if (utilisateur.TypeCompte != TypeCompte.ClientCarbone)
        {
            return (utilisateur, RedirectToPage("/Dashboard/Index"));
        }

        Dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (Dossier?.StatutQuestionnaire is null or StatutQuestionnaireCollecte.Soumis)
        {
            return (utilisateur, RedirectToPage("/EspaceClient/Index"));
        }

        return (utilisateur, null);
    }
}
