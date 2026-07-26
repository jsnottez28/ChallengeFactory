using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class MonParcoursModel(
    ICohorteService cohorteService,
    IPreuveService preuveService,
    IForumService forumService,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public List<ParcoursEnCoursInfo> Parcours { get; private set; } = [];

    // Cle = ChallengeEtapeId, pour retrouver le contexte de chaque parcours affiche dans la
    // colonne de droite (prompt section F) sans multiplier les proprietes paralleles.
    public Dictionary<int, ParcoursContexteInfo> Contextes { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        Parcours = await cohorteService.GetMesParcoursEnCoursAsync(userId);

        foreach (var parcours in Parcours)
        {
            var maPreuve = await preuveService.GetMaPreuveAsync(userId, parcours.ChallengeEtapeId);

            var messages = await forumService.GetMessagesEtapeAsync(parcours.ChallengeEtapeId, parcours.CohorteId, userId);
            var dernierMessage = Aplatir(messages).OrderByDescending(m => m.DateCreation).FirstOrDefault();

            var preuvesAValider = await preuveService.GetPreuvesAValiderAsync(userId, parcours.CohorteId);
            var prochaineVisio = await cohorteService.GetProchaineVisioAsync(parcours.CohorteId);

            Contextes[parcours.ChallengeEtapeId] = new ParcoursContexteInfo
            {
                MaPreuve = maPreuve,
                DernierMessageForum = dernierMessage,
                NombrePreuvesAValider = preuvesAValider.Count,
                ProchaineVisio = prochaineVisio,
            };
        }

        return Page();
    }

    private static List<ForumMessageInfo> Aplatir(List<ForumMessageInfo> messages)
    {
        var tous = new List<ForumMessageInfo>();
        foreach (var message in messages)
        {
            tous.Add(message);
            tous.AddRange(Aplatir(message.Reponses));
        }
        return tous;
    }

    public sealed class ParcoursContexteInfo
    {
        public PreuveDetailInfo? MaPreuve { get; set; }
        public ForumMessageInfo? DernierMessageForum { get; set; }
        public int NombrePreuvesAValider { get; set; }
        public VisioEtapeInfo? ProchaineVisio { get; set; }
    }
}
