using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Web.Controllers;

namespace Integration.Controllers;

// Non-regression explicite (cf. prompt "Depot de preuves, points et forum", section 9) :
// la vue "Contributions de la cohorte" (agregats Points_Karma/XP_Savoir par membre) ne
// doit jamais etre accessible a un compte disposant du seul role Apprenant. Dans cette
// codebase, l'acces aux vues gestionnaire est gouverne exclusivement par
// [Authorize(Policy = "Droit:...")] au niveau des actions de controleur (jamais un
// classement generique par role - cf. DroitAuthorizationHandler) : ce test verifie que
// l'attribut requis est bien present et n'a pas ete perdu au fil d'un refactor futur.
public class PreuvesControllerAuthorizationTests
{
    private static AuthorizeAttribute ObtenirAuthorizeAttribute(string nomAction)
    {
        var methode = typeof(PreuvesController).GetMethod(nomAction, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Action \"{nomAction}\" introuvable sur PreuvesController.");

        return methode.GetCustomAttribute<AuthorizeAttribute>()
            ?? throw new InvalidOperationException($"Action \"{nomAction}\" n'a pas d'attribut [Authorize].");
    }

    [Fact]
    public void Contributions_ExigeLeDroitPreuveConsulter_JamaisAccessibleAUnSimpleApprenant()
    {
        var attribut = ObtenirAuthorizeAttribute(nameof(PreuvesController.Contributions));
        Assert.Equal("Droit:PREUVE.CONSULTER", attribut.Policy);
    }

    [Fact]
    public void Etape_ExigeLeDroitPreuveConsulter()
    {
        var attribut = ObtenirAuthorizeAttribute(nameof(PreuvesController.Etape));
        Assert.Equal("Droit:PREUVE.CONSULTER", attribut.Policy);
    }

    [Fact]
    public void Valider_ExigeLeDroitPreuveValider()
    {
        var attribut = ObtenirAuthorizeAttribute(nameof(PreuvesController.Valider));
        Assert.Equal("Droit:PREUVE.VALIDER", attribut.Policy);
    }

    [Fact]
    public void Refuser_ExigeLeDroitPreuveValider()
    {
        var attribut = ObtenirAuthorizeAttribute(nameof(PreuvesController.Refuser));
        Assert.Equal("Droit:PREUVE.VALIDER", attribut.Policy);
    }

    [Fact]
    public void SupprimerMessage_ExigeLeDroitForumSupprimer()
    {
        var attribut = ObtenirAuthorizeAttribute(nameof(PreuvesController.SupprimerMessage));
        Assert.Equal("Droit:FORUM.SUPPRIMER", attribut.Policy);
    }
}
