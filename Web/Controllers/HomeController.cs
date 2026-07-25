using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalServices.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Data;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private const string EmailDestinataireContact = "jsnottez@modulo-training.com";

        private readonly ILogger<HomeController> _logger;
        private readonly IEmailService _emailService;
        private readonly ICohorteService _cohorteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            IEmailService emailService,
            ICohorteService cohorteService,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _emailService = emailService;
            _cohorteService = cohorteService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("la-methode")]
        public IActionResult Methode()
        {
            return View();
        }

        [Route("a-propos")]
        public IActionResult APropos()
        {
            return View();
        }

        [HttpGet]
        [Route("contact")]
        public IActionResult Contact()
        {
            return View(new ContactFormModel());
        }

        [HttpPost]
        [Route("contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var corpsEmail = EmailTemplates.MessageContact(model.Nom, model.Email, model.Message);
            await _emailService.EnvoyerAsync(EmailDestinataireContact, $"Nouveau message de {model.Nom}", corpsEmail);

            TempData["ContactEnvoye"] = true;
            return RedirectToAction(nameof(Contact));
        }

        [HttpGet]
        [Route("formations")]
        public async Task<IActionResult> Formations()
        {
            var toutesLesCohortes = await _cohorteService.GetAllAsync();
            var sessionsOuvertes = toutesLesCohortes
                .Where(c => c.ChallengeMode == ModePlateforme.BtoC && c.Statut == StatutCohorte.EnPreparation)
                .ToList();

            return View(sessionsOuvertes);
        }

        [HttpPost]
        [Route("formations/{cohorteId:int}/inscription")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InscriptionFormation(int cohorteId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Register", new { area = "Identity", cohorteId });
            }

            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                return RedirectToPage("/Account/Register", new { area = "Identity", cohorteId });
            }

            var (success, errorMessage) = await _cohorteService.AutoInscrireAsync(cohorteId, userId);
            TempData["StatusMessage"] = success
                ? "Inscription enregistrée ! Retrouvez votre parcours depuis votre tableau de bord dès son lancement."
                : errorMessage;

            return RedirectToAction(nameof(Formations));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ContactFormModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre nom.")]
        [StringLength(150)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Merci d'indiquer votre email.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Merci de décrire votre besoin.")]
        [StringLength(4000)]
        [Display(Name = "Votre message")]
        public string Message { get; set; } = string.Empty;
    }
}
