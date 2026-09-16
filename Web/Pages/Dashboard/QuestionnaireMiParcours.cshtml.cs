using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class QuestionnaireMiParcoursModel(
    IQuestionnaireMiParcoursService questionnaireMiParcoursService,
    ICohorteService cohorteService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int CohorteId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Merci d'indiquer si le projet vous semble toujours pertinent.")]
    public PertinenceProjet? ProjetToujoursPertinent { get; set; }

    [BindProperty]
    [Range(0, 10, ErrorMessage = "La note doit être comprise entre 0 et 10.")]
    public int NoteAccompagnement { get; set; } = 5;

    [BindProperty]
    [Display(Name = "Difficultés rencontrées")]
    public string? DifficultesRencontrees { get; set; }

    [BindProperty]
    [Display(Name = "Ajustements souhaités pour la suite")]
    public string? AjustementsSouhaites { get; set; }

    public string? ChallengeTitre { get; private set; }
    public bool CohorteIntrouvable { get; private set; }
    public bool ADejaRepondu { get; private set; }
    public string? StatusMessage { get; private set; }

    public async Task OnGetAsync()
    {
        var cohorte = await cohorteService.GetResumeAsync(CohorteId);
        if (cohorte is null)
        {
            CohorteIntrouvable = true;
            return;
        }

        ChallengeTitre = cohorte.ChallengeTitre;

        var utilisateurId = userManager.GetUserId(User)!;
        ADejaRepondu = await questionnaireMiParcoursService.ADejaReponduAsync(CohorteId, utilisateurId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var cohorte = await cohorteService.GetResumeAsync(CohorteId);
        if (cohorte is null)
        {
            CohorteIntrouvable = true;
            return Page();
        }

        ChallengeTitre = cohorte.ChallengeTitre;

        var utilisateurId = userManager.GetUserId(User)!;
        ADejaRepondu = await questionnaireMiParcoursService.ADejaReponduAsync(CohorteId, utilisateurId);
        if (ADejaRepondu)
        {
            return Page();
        }

        if (!ModelState.IsValid || ProjetToujoursPertinent is null)
        {
            return Page();
        }

        var (success, errorMessage) = await questionnaireMiParcoursService.EnregistrerReponseAsync(
            CohorteId, utilisateurId, ProjetToujoursPertinent.Value, NoteAccompagnement, DifficultesRencontrees, AjustementsSouhaites);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible d'enregistrer votre réponse.");
            return Page();
        }

        ADejaRepondu = true;
        StatusMessage = "Merci, votre point d'étape a bien été enregistré.";
        return Page();
    }
}
