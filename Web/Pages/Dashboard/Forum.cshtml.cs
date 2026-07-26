using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

[Authorize]
public class ForumModel(IForumService forumService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int CohorteId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ChallengeEtapeId { get; set; }

    [BindProperty]
    public string Contenu { get; set; } = string.Empty;

    [BindProperty]
    public int? MessageParentId { get; set; }

    public List<EtapeForumInfo> Etapes { get; private set; } = [];
    public List<ForumMessageInfo> Messages { get; private set; } = [];
    public EtapeForumInfo? EtapeAffichee { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        await ChargerAsync(userId);
        return Page();
    }

    public async Task<IActionResult> OnPostPosterAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        if (ChallengeEtapeId is null)
        {
            return BadRequest();
        }

        var lienForum = Url.Page("/Dashboard/Forum", null, new { CohorteId, ChallengeEtapeId }, Request.Scheme) ?? "/Dashboard/Forum";
        var (success, errorMessage) = await forumService.PosterMessageAsync(userId, CohorteId, ChallengeEtapeId.Value, Contenu, MessageParentId, lienForum);
        StatusMessage = success ? null : errorMessage;

        return RedirectToPage(new { CohorteId, ChallengeEtapeId });
    }

    public async Task<IActionResult> OnPostMarquerUtileAsync(int messageId)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var lienForum = Url.Page("/Dashboard/Forum", null, new { CohorteId, ChallengeEtapeId }, Request.Scheme) ?? "/Dashboard/Forum";
        var (success, errorMessage) = await forumService.MarquerUtileAsync(messageId, userId, lienForum);
        StatusMessage = success ? "Message marqué comme utile." : errorMessage;

        return RedirectToPage(new { CohorteId, ChallengeEtapeId });
    }

    private async Task ChargerAsync(string userId)
    {
        Etapes = await forumService.GetEtapesAccessiblesAsync(CohorteId, userId);

        EtapeAffichee = ChallengeEtapeId is int id
            ? Etapes.FirstOrDefault(e => e.ChallengeEtapeId == id)
            : Etapes.FirstOrDefault(e => e.EstEtapeCourante) ?? Etapes.LastOrDefault();

        if (EtapeAffichee is not null)
        {
            ChallengeEtapeId = EtapeAffichee.ChallengeEtapeId;
            Messages = await forumService.GetMessagesEtapeAsync(EtapeAffichee.ChallengeEtapeId, CohorteId, userId);
        }
    }
}
