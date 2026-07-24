using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Web.Security;

namespace Web.TagHelpers;

/// <summary>
/// Masque son contenu si l'utilisateur courant n'a pas le droit indique.
/// Usage : &lt;droit-visible code="ORGANISATION.CREER"&gt;...&lt;/droit-visible&gt;
/// </summary>
[HtmlTargetElement("droit-visible")]
public sealed class DroitVisibleTagHelper(IAuthorizationService authorizationService) : TagHelper
{
    [HtmlAttributeName("code")]
    public string Code { get; set; } = string.Empty;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        if (string.IsNullOrWhiteSpace(Code))
        {
            output.SuppressOutput();
            return;
        }

        var resultat = await authorizationService.AuthorizeAsync(
            ViewContext.HttpContext.User,
            $"{DroitPolicyProvider.Prefix}{Code}");

        if (!resultat.Succeeded)
        {
            output.SuppressOutput();
        }
    }
}
