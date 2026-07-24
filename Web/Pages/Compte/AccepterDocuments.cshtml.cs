using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Compte;

[Authorize]
public class AccepterDocumentsModel(IDocumentLegalService documentLegalService, UserManager<ApplicationUser> userManager) : PageModel
{
    public List<DocumentAAccepterViewModel> Documents { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await ChargerDocumentsAsync();
        return Documents.Count == 0 ? RedirectVersRetour() : Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] List<TypeDocumentLegal>? typesAcceptes)
    {
        await ChargerDocumentsAsync();

        if (Documents.Count == 0)
        {
            return RedirectVersRetour();
        }

        var typesCoches = typesAcceptes ?? [];
        if (!Documents.All(document => typesCoches.Contains(document.Type)))
        {
            ModelState.AddModelError(string.Empty, "Vous devez accepter chaque document pour continuer.");
            return Page();
        }

        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return RedirectVersRetour();
        }

        var adresseIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        foreach (var document in Documents)
        {
            await documentLegalService.AccepterAsync(userId, document.Type, adresseIp);
        }

        return RedirectVersRetour();
    }

    private IActionResult RedirectVersRetour()
    {
        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("/Dashboard/Index");
    }

    private async Task ChargerDocumentsAsync()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            Documents = [];
            return;
        }

        var typesEnAttente = await documentLegalService.GetTypesEnAttenteAsync(userId);

        Documents = [];
        foreach (var type in typesEnAttente)
        {
            var document = await documentLegalService.GetVersionPublieeAsync(type);
            if (document is null)
            {
                continue;
            }

            Documents.Add(new DocumentAAccepterViewModel
            {
                Type = type,
                Version = document.Version,
                DateEffective = document.DateEffective,
                ContenuHtml = Markdig.Markdown.ToHtml(document.Contenu),
            });
        }
    }

    public sealed class DocumentAAccepterViewModel
    {
        public TypeDocumentLegal Type { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime DateEffective { get; set; }
        public string ContenuHtml { get; set; } = string.Empty;
    }
}
