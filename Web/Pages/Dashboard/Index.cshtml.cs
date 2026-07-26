using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Page d'accueil/atterrissage de l'apprenant apres connexion (prompt section E) - distincte
// de "Mon parcours en cours" qui reste la vue detaillee par Challenge/Cohorte. Synthetise :
// Cohorte(s) active(s), totaux de points, apercu compact de l'etape courante, statut des
// preuves en cours. Egalement le nouvel emplacement des badges sociaux ("Super Helper")
// suite a la suppression de la page dediee "Mes badges" (prompt section G).
[Authorize]
public class IndexModel(
    ICohorteService cohorteService,
    IPreuveService preuveService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public List<ParcoursEnCoursInfo> ParcoursEnCours { get; private set; } = [];
    public PointsResumeInfo Points { get; private set; } = new();
    public List<BadgeSocialInfo> Badges { get; private set; } = [];

    public int NombreSoumises { get; private set; }
    public int NombreValideesParLesPairs { get; private set; }
    public int NombreEnAttenteDeMonAction { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        ParcoursEnCours = await cohorteService.GetMesParcoursEnCoursAsync(userId);
        Points = await preuveService.GetMesPointsAsync(userId);
        Badges = await preuveService.GetMesBadgesAsync(userId);

        foreach (var parcours in ParcoursEnCours)
        {
            var maPreuve = await preuveService.GetMaPreuveAsync(userId, parcours.ChallengeEtapeId);
            if (maPreuve is null)
            {
                continue;
            }

            switch (maPreuve.Statut)
            {
                case StatutPreuve.Soumise:
                    NombreSoumises++;
                    // Aucun champ dedie ne distingue "en attente de premier avis" de
                    // "un pair a demande une revision" : on le deduit de la presence d'un
                    // retour "À revoir" tant que la preuve reste Soumise (pas encore
                    // re-soumise/re-validee).
                    if (maPreuve.Retours.Exists(r => r.Decision == "À revoir"))
                    {
                        NombreEnAttenteDeMonAction++;
                    }
                    break;
                case StatutPreuve.ValideeParLesPairs:
                    NombreValideesParLesPairs++;
                    break;
            }
        }

        return Page();
    }
}
