using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[AllowAnonymous]
public class LegalController(IDocumentLegalService documentLegalService) : Controller
{
    [HttpGet("/cgu")]
    public async Task<IActionResult> Cgu()
    {
        var document = await documentLegalService.GetVersionPublieeAsync(TypeDocumentLegal.CGU);
        return View("Consulter", document);
    }

    [HttpGet("/confidentialite")]
    public async Task<IActionResult> Confidentialite()
    {
        var document = await documentLegalService.GetVersionPublieeAsync(TypeDocumentLegal.PPD);
        return View("Consulter", document);
    }
}
