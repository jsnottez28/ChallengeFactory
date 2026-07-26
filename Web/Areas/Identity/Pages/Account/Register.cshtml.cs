// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalServices.Email;
using Web.Data;

namespace Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IDocumentLegalService _documentLegalService;
    private readonly ICohorteService _cohorteService;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        IDocumentLegalService documentLegalService,
        ICohorteService cohorteService)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _documentLegalService = documentLegalService;
        _cohorteService = cohorteService;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ReturnUrl { get; set; }

    // Renseigne quand l'inscription part du catalogue public /formations (auto-inscription
    // BtoC sur une Cohorte En preparation) - voir HomeController.InscriptionFormation.
    public int? CohorteId { get; set; }

    // Renseigne quand l'inscription part d'une demande d'embarquement sur un Challenge sans
    // Cohorte encore ouverte - voir HomeController.DemanderEmbarquement (prompt section H).
    public int? ChallengeId { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Vous devez accepter les CGU pour créer un compte.")]
        [Display(Name = "J'accepte les conditions générales d'utilisation")]
        public bool AccepteCgu { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Vous devez accepter la politique de protection des données pour créer un compte.")]
        [Display(Name = "J'accepte la politique de protection des données")]
        public bool AcceptePpd { get; set; }
    }


    public async Task OnGetAsync(string? returnUrl = null, int? cohorteId = null, int? challengeId = null)
    {
        ReturnUrl = returnUrl;
        CohorteId = cohorteId;
        ChallengeId = challengeId;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null, int? cohorteId = null, int? challengeId = null)
    {
        CohorteId = cohorteId;
        ChallengeId = challengeId;
        var localReturnUrl = returnUrl ?? Url.Content("~/")!;
        if (localReturnUrl == Url.Content("~/"))
        {
            localReturnUrl = Url.Page("/Dashboard/Index", new { area = "" });
        }

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = CreateUser();
            // Inscription BtoC en autonomie : le compte reste "Modere" (en attente de
            // validation admin) tant qu'un administrateur ne l'a pas active manuellement
            // (voir ApplicationSignInManager.CanSignInAsync et Admin/Utilisateurs).
            user.Statut = StatutUtilisateur.Modere;
            user.Mode = ModePlateforme.BtoC;

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);

                var adresseIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _documentLegalService.AccepterAsync(userId, TypeDocumentLegal.CGU, adresseIp);
                await _documentLegalService.AccepterAsync(userId, TypeDocumentLegal.PPD, adresseIp);

                if (cohorteId.HasValue)
                {
                    // L'inscription a la Cohorte n'est jamais bloquee par la confirmation
                    // d'email a venir : seul l'acces au contenu depend du statut d'acces
                    // plateforme (voir ICohorteService.AutoInscrireAsync et CLAUDE.md).
                    await _cohorteService.AutoInscrireAsync(cohorteId.Value, userId);
                }
                else if (challengeId.HasValue)
                {
                    // Meme logique que ci-dessus, pour la demande d'embarquement (Cohorte
                    // Proposee, cf. ICohorteService.DemanderEmbarquementAsync) : ne bloque
                    // jamais sur la confirmation d'email a venir.
                    await _cohorteService.DemanderEmbarquementAsync(challengeId.Value, userId);
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme)!;

                var nomUtilisateur = user.Prenom ?? Input.Email;
                var corpsEmail = EmailTemplates.ConfirmationEmail(nomUtilisateur, callbackUrl);
                await _emailSender.SendEmailAsync(Input.Email, "Confirmez votre adresse email — Challenges Factory", corpsEmail);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(localReturnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}
