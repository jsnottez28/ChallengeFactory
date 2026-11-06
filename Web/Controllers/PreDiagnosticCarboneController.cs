using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.Data;

namespace Web.Controllers;

// Pre-diagnostic carbone : estimation rapide en approche monetaire, accessible sans
// compte, distincte du Bilan Carbone(R) complet reserve a une prestation avec un praticien
// - cf. IPreBilanCarboneService pour le detail de la demarche et du choix de vocabulaire.
[Route("pre-diagnostic-carbone")]
public class PreDiagnosticCarboneController(
    IPreBilanCarboneService preBilanService,
    ICollecteCarboneService collecteService,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : Controller
{
    private const string CleTempDataDernierPreBilan = "DernierPreBilanId";

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var facteurs = await preBilanService.GetFacteursDisponiblesAsync();
        ViewData["Facteurs"] = facteurs;

        var model = new PreDiagnosticCarboneFormModel { AnneeReference = DateTime.UtcNow.Year };

        if (signInManager.IsSignedIn(User))
        {
            var utilisateur = await userManager.GetUserAsync(User);
            if (utilisateur is not null)
            {
                // Point B : coordonnees deja connues, pas besoin de les redemander - seules
                // les donnees propres a l'entreprise (effectif, CA...) restent a saisir.
                model.Email = utilisateur.Email ?? string.Empty;
                model.Nom = utilisateur.Nom;
                model.Prenom = utilisateur.Prenom;
                model.Telephone = utilisateur.Telephone;
            }
        }

        ViewData["EstConnecte"] = signInManager.IsSignedIn(User);
        return View(model);
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PreDiagnosticCarboneFormModel model)
    {
        var facteurs = await preBilanService.GetFacteursDisponiblesAsync();
        ViewData["Facteurs"] = facteurs;
        var estConnecte = signInManager.IsSignedIn(User);
        ViewData["EstConnecte"] = estConnecte;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var valeurs = new Dictionary<string, decimal>();
        foreach (var facteur in facteurs)
        {
            if (model.Valeurs.TryGetValue(facteur.Code, out var valeur) && valeur is > 0)
            {
                valeurs[facteur.Code] = valeur.Value;
            }
        }

        var input = new PreBilanCarboneInput
        {
            Email = model.Email,
            Nom = model.Nom,
            Prenom = model.Prenom,
            Societe = model.Societe,
            Telephone = model.Telephone,
            SecteurActivite = model.SecteurActivite,
            EffectifEtp = model.EffectifEtp,
            ChiffreAffairesKEuros = model.ChiffreAffairesKEuros,
            AnneeReference = model.AnneeReference,
            ValeursParCodeFacteur = valeurs,
        };

        if (estConnecte)
        {
            // Les champs coordonnees sont masques cote vue quand on est connecte ; on ne
            // fait jamais confiance a ce qui a pu etre poste pour ces champs-la, on les
            // reprend directement du compte (cf. point B).
            var utilisateur = await userManager.GetUserAsync(User);
            if (utilisateur is not null)
            {
                input.Email = utilisateur.Email ?? input.Email;
                input.Nom = utilisateur.Nom;
                input.Prenom = utilisateur.Prenom;
                input.Telephone = utilisateur.Telephone;
            }
        }

        var (success, errorMessage, resultat) = await preBilanService.DeposerAsync(input);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Une erreur est survenue.");
            return View(model);
        }

        if (!estConnecte)
        {
            // Point C : permet au visiteur non connecte de creer son espace client juste
            // apres, sans qu'il (ou quiconque) puisse le faire pour un autre Pre-diagnostic
            // en devinant un Id - seul ce TempData (lie a son navigateur) en fait foi.
            TempData[CleTempDataDernierPreBilan] = resultat!.Id;
        }

        return View("Resultat", resultat);
    }

    // Point C : le visiteur qui vient de deposer son Pre-diagnostic (pas connecte) cree
    // lui-meme son Espace Client, sans attendre qu'un Gestionnaire le fasse depuis le CRM.
    // Rerend directement "Resultat" (pas de redirection) pour ne pas lui faire perdre de
    // vue son estimation - meme principe que le POST Index d'origine.
    [HttpPost("creer-mon-espace")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreerMonEspace()
    {
        if (TempData.Peek(CleTempDataDernierPreBilan) is not int preBilanId)
        {
            return RedirectToAction(nameof(Index));
        }

        var resultat = await preBilanService.GetResultatAsync(preBilanId);
        if (resultat is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var (success, errorMessage) = await collecteService.OuvrirCompteClientSelfServiceAsync(preBilanId, LienActivation, LienConnexion());

        if (success)
        {
            ViewData["EspaceClientMessage"] = "Votre espace est créé : un email vous a été envoyé pour y accéder.";
            TempData.Remove(CleTempDataDernierPreBilan);
        }
        else
        {
            ViewData["EspaceClientErreur"] = errorMessage;
            // On garde le TempData vivant pour permettre un nouvel essai (ex. apres avoir
            // change d'adresse email depuis la fiche CRM) plutot que de le laisser expirer.
            TempData[CleTempDataDernierPreBilan] = preBilanId;
        }

        return View("Resultat", resultat);
    }

    private string LienActivation(string token) =>
        Url.Page("/Compte/DefinirMotDePasse", null, new { token }, Request.Scheme) ?? $"/Compte/DefinirMotDePasse?token={token}";

    private string LienConnexion() =>
        Url.Page("/Account/Login", null, new { area = "Identity" }, Request.Scheme) ?? "/Identity/Account/Login";

    public class PreDiagnosticCarboneFormModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre email.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Nom")]
        public string? Nom { get; set; }

        [StringLength(150)]
        [Display(Name = "Prénom")]
        public string? Prenom { get; set; }

        [StringLength(200)]
        [Display(Name = "Société")]
        public string? Societe { get; set; }

        [StringLength(30)]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }

        [StringLength(150)]
        [Display(Name = "Secteur d'activité")]
        public string? SecteurActivite { get; set; }

        [Range(0, 1_000_000)]
        [Display(Name = "Effectif (ETP)")]
        public int? EffectifEtp { get; set; }

        [Range(0, 10_000_000)]
        [Display(Name = "Chiffre d'affaires (k€)")]
        public decimal? ChiffreAffairesKEuros { get; set; }

        // Annee sur laquelle portent les donnees saisies (ex. l'exercice comptable) -
        // permet de rapprocher plusieurs depots d'une meme societe pour suivre son
        // evolution. Bornes larges pour couvrir un exercice decale sur l'annee suivante.
        [Required(ErrorMessage = "Merci d'indiquer l'année de référence des données saisies.")]
        [Range(2000, 2100, ErrorMessage = "Année invalide.")]
        [Display(Name = "Année de référence")]
        public int? AnneeReference { get; set; }

        [Required(ErrorMessage = "Merci d'accepter que vos coordonnées soient utilisées pour vous recontacter au sujet de ce pré-diagnostic.")]
        [Display(Name = "Consentement")]
        public bool ConsentementRgpd { get; set; }

        // Cle = FacteurEmissionInfo.Code, lu depuis Request.Form["Valeurs[CODE]"] via le
        // binder par defaut - meme principe que les autres formulaires dynamiques de la
        // plateforme (montants/quantites par categorie, jamais de liste fixe en dur cote
        // vue). decimal? (et non decimal) : un champ laisse vide soumet une chaine vide,
        // que le binder ASP.NET Core refuse pour un type valeur non-nullable ("The value
        // '' is invalid.", un par champ vide) - nullable, il la traite simplement comme
        // "non renseigne" (null), sans erreur de validation.
        public Dictionary<string, decimal?> Valeurs { get; set; } = [];
    }
}
