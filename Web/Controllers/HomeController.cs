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
        private readonly IChallengeService _challengeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            IEmailService emailService,
            ICohorteService cohorteService,
            IChallengeService challengeService,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _emailService = emailService;
            _cohorteService = cohorteService;
            _challengeService = challengeService;
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

            var challenges = await _challengeService.GetAllAsync();
            var challengesDisponibles = challenges
                .Where(c => c.Mode == ModePlateforme.BtoC && c.Statut == StatutChallenge.Publie)
                .OrderBy(c => c.Titre)
                .Select(challenge => new FormationViewModel
                {
                    ChallengeId = challenge.Id,
                    Titre = challenge.Titre,
                    Slogan = challenge.Slogan,
                    Description = challenge.Description,
                    NombreEtapes = challenge.NombreEtapes,
                    CohortesOuvertes = sessionsOuvertes.Where(c => c.ChallengeId == challenge.Id).ToList(),
                })
                .ToList();

            return View(challengesDisponibles);
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

        // "Demander un embarquement pour ce Challenge" (prompt section H) : cree ou rejoint
        // une Cohorte Proposee, toujours soumise a validation humaine avant de devenir une
        // vraie session ouverte (jamais d'automatisation qui contourne le Gestionnaire).
        [HttpPost]
        [Route("formations/{challengeId:int}/demander-embarquement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemanderEmbarquement(int challengeId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Register", new { area = "Identity", challengeId });
            }

            var userId = _userManager.GetUserId(User);
            if (userId is null)
            {
                return RedirectToPage("/Account/Register", new { area = "Identity", challengeId });
            }

            var (success, errorMessage, _) = await _cohorteService.DemanderEmbarquementAsync(challengeId, userId);
            TempData["StatusMessage"] = success
                ? "Ta demande d'embarquement a été enregistrée ! Notre équipe l'étudie et reviendra vers toi dès qu'une session est prête."
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

    // Vue Challenge-centree du catalogue public (prompt section H) : un Challenge, sa
    // Description, et les Cohortes actuellement ouvertes a l'inscription pour ce Challenge
    // (jamais une Cohorte Proposee - cf. ICohorteService.GetAllAsync).
    public class FormationViewModel
    {
        public int ChallengeId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Slogan { get; set; }
        public string? Description { get; set; }
        public int NombreEtapes { get; set; }
        public List<CohorteResume> CohortesOuvertes { get; set; } = [];
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
