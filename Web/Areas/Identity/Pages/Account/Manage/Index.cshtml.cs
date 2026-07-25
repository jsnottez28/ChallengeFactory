// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? Username { get; set; }

    public string? Email { get; set; }

    public StatutUtilisateur Statut { get; set; }

    public DateTime CreeLe { get; set; }

    public List<SelectListItem> Civilites { get; set; } = [];

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

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
    public class InputModel
    {
        [Display(Name = "Prénom")]
        public string? Prenom { get; set; }

        [Display(Name = "Nom")]
        public string? Nom { get; set; }

        [Display(Name = "Civilite")]
        public int? CiviliteId { get; set; }

        [Phone]
        [Display(Name = "Téléphone")]
        public string? Telephone { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        var email = await _userManager.GetEmailAsync(user);

        Username = userName;
        Email = email;
        CreeLe = user.CreeLe;
        Statut = user.Statut;
        Civilites = await _context.Civilites
            .Where(civilite => civilite.EstActif)
            .OrderBy(civilite => civilite.Ordre)
            .Select(civilite => new SelectListItem
            {
                Value = civilite.Id.ToString(),
                Text = civilite.LibelleLong
            })
            .ToListAsync();

        Input = new InputModel
        {
            Prenom = user.Prenom,
            Nom = user.Nom,
            CiviliteId = user.CiviliteId,
            Telephone = user.Telephone ?? await _userManager.GetPhoneNumberAsync(user)
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        if (Input.CiviliteId.HasValue)
        {
            var civiliteExists = await _context.Civilites
                .AnyAsync(civilite => civilite.Id == Input.CiviliteId.Value && civilite.EstActif);
            if (!civiliteExists)
            {
                ModelState.AddModelError($"{nameof(Input)}.{nameof(InputModel.CiviliteId)}", "La civilite selectionnee n'est pas valide.");
                await LoadAsync(user);
                return Page();
            }
        }

        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.Telephone != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.Telephone);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Error: Une erreur est survenue lors de la mise à jour du téléphone.";
                return RedirectToPage();
            }
        }

        user.Prenom = Input.Prenom;
        user.Nom = Input.Nom;
        user.CiviliteId = Input.CiviliteId;
        user.Telephone = Input.Telephone;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            StatusMessage = "Error: Une erreur est survenue lors de la mise à jour du profil.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Le profil a été mis à jour.";
        return RedirectToPage();
    }
}
