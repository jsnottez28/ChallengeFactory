using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Admin;

// Tableau de bord global de preparation/passage d'un audit Qualiopi - agrege toutes les
// Cohortes confondues (cf. IQualiopiDashboardService pour le detail de chaque indicateur
// et ses sources). Distinct des blocs de stats par Cohorte deja presents sur
// CohortesController.Details, qui restent le point d'entree pour le detail nominatif.
[Authorize(Policy = "Droit:QUALIOPI.CONSULTER")]
public class TableauDeBordQualiopiModel(IQualiopiDashboardService qualiopiDashboardService) : PageModel
{
    public QualiopiTableauDeBordInfo TableauDeBord { get; private set; } = new();

    public async Task OnGetAsync()
    {
        TableauDeBord = await qualiopiDashboardService.GetTableauDeBordAsync();
    }
}
