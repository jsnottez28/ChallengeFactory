using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.ExternalServices.Email;
using Infrastructure.ExternalServices.Recaptcha;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly IReclamationService _reclamationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRecaptchaService _recaptchaService;
        private readonly RecaptchaSettings _recaptchaSettings;

        public HomeController(
            ILogger<HomeController> logger,
            IEmailService emailService,
            ICohorteService cohorteService,
            IChallengeService challengeService,
            IReclamationService reclamationService,
            UserManager<ApplicationUser> userManager,
            IRecaptchaService recaptchaService,
            IOptions<RecaptchaSettings> recaptchaOptions)
        {
            _logger = logger;
            _emailService = emailService;
            _cohorteService = cohorteService;
            _challengeService = challengeService;
            _reclamationService = reclamationService;
            _userManager = userManager;
            _recaptchaService = recaptchaService;
            _recaptchaSettings = recaptchaOptions.Value;
        }

        // Expose la cle publique reCAPTCHA aux vues Contact/Accessibilite - tant que
        // Recaptcha:SiteKey n'est pas configuree (cle non encore creee cote Google), le widget
        // ne s'affiche pas et la verification serveur est elle-meme desactivee (cf.
        // GoogleRecaptchaService), pour ne jamais bloquer un environnement sans credentials.
        private void ExposerCleRecaptcha() => ViewData["RecaptchaSiteKey"] = _recaptchaSettings.SiteKey;

        public IActionResult Index()
        {
            return View();
        }

        [Route("bilan-carbone")]
        public IActionResult BilanCarbone()
        {
            return View();
        }

        [Route("bilan-carbone-eure-et-loir")]
        public IActionResult BilanCarboneEureEtLoir()
        {
            return View();
        }

        // Page vitrine generaliste sur les ecolabels et labels environnementaux (les 2
        // ecolabels officiels ISO 14024 - NF Environnement, Ecolabel europeen - plus les
        // labels sectoriels reconnus comme la Clef Verte ou COSMOS/Ecocert) : remplace une
        // premiere version ciblee uniquement sur les hebergeurs du Perche, jugee trop etroite
        // (un secteur = un exemple parmi d'autres ici, pas le sujet de la page).
        [Route("ecolabels-labels-environnementaux")]
        public IActionResult EcolabelsEtLabelsEnvironnementaux()
        {
            return View();
        }

        // Page vitrine ciblant les PME/ETI sous-traitantes (dont la filiere cosmetique de
        // Chartres) qui doivent produire un volet environnement pour un label/evaluation RSE
        // (EcoVadis, Lucie 26000, Engage RSE...). L'ACV simplifiee a sa propre page dediee
        // (AcvSimplifiee) plutot qu'une simple section ici.
        [Route("label-rse-ecovadis")]
        public IActionResult LabelRseEcovadis()
        {
            return View();
        }

        // Page vitrine dediee a l'ACV simplifiee d'un produit (ordre de grandeur des impacts,
        // points chauds du cycle de vie) - distincte d'une ACV complete conforme ISO
        // 14040/14044 avec revue critique, pour laquelle on oriente vers des partenaires.
        [Route("acv-simplifiee")]
        public IActionResult AcvSimplifiee()
        {
            return View();
        }

        // Bilan de competences (reconversion vers un metier de la transition ecologique) :
        // dispositif distinct du Bilan Carbone(R), encadre par le Code du travail (art.
        // L6313-4). Page de presentation statique, meme principe que les autres pages
        // vitrine - aucune donnee metier (dossiers de beneficiaires, etc.) ne transite ici,
        // uniquement de la prise de contact via le meme calendrier de pre-diagnostic.
        [Route("bilan-de-competences")]
        public IActionResult BilanDeCompetences()
        {
            return View();
        }

        // Le Lab : pas de modele Article en base (aucune migration sans accord explicite,
        // cf. mission) - contenu statique, meme technique que La methode/A propos. Chaque
        // article est une action/vue dediee plutot qu'un systeme de slug generique, le
        // volume (3 articles fixes) ne justifie pas plus.
        [Route("le-lab")]
        public IActionResult LeLab()
        {
            return View();
        }

        [Route("le-lab/beges-obligatoire-2026")]
        public IActionResult LeLabBegesObligatoire()
        {
            return View();
        }

        [Route("le-lab/diag-decarbon-action")]
        public IActionResult LeLabDiagDecarbonAction()
        {
            return View();
        }

        [Route("le-lab/plans-de-transition-dans-un-tiroir")]
        public IActionResult LeLabPlansTransition()
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

        // Page statique (hors DocumentLegal) : contenu fixe sans logique de versionnement/
        // reacceptation, a la difference de /cgu et /confidentialite (cf. LegalController).
        [Route("mentions-legales")]
        public IActionResult MentionsLegales()
        {
            return View();
        }

        // Page publique + canal de contact dedie, exigence Qualiopi indicateur 26
        // (referent handicap identifiable). Meme circuit email que le formulaire de
        // contact general (aucune boite dediee distincte pour l'instant) - le sujet
        // prefixe suffit a distinguer une demande accessibilite en reception.
        [HttpGet]
        [Route("accessibilite")]
        public IActionResult Accessibilite()
        {
            ExposerCleRecaptcha();
            return View(new ContactFormModel());
        }

        [HttpPost]
        [Route("accessibilite")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accessibilite(ContactFormModel model)
        {
            ExposerCleRecaptcha();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await _recaptchaService.ValiderAsync(Request.Form["g-recaptcha-response"]))
            {
                ModelState.AddModelError(string.Empty, "Merci de confirmer que vous n'êtes pas un robot.");
                return View(model);
            }

            var corpsEmail = EmailTemplates.MessageContact(model.Nom, model.Email, model.Message);
            await _emailService.EnvoyerAsync(EmailDestinataireContact, $"[Référent handicap] Nouveau message de {model.Nom}", corpsEmail);

            TempData["ContactEnvoye"] = true;
            return RedirectToAction(nameof(Accessibilite));
        }

        [HttpGet]
        [Route("contact")]
        public IActionResult Contact()
        {
            ExposerCleRecaptcha();
            return View(new ContactFormModel());
        }

        [HttpPost]
        [Route("contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactFormModel model)
        {
            ExposerCleRecaptcha();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!await _recaptchaService.ValiderAsync(Request.Form["g-recaptcha-response"]))
            {
                ModelState.AddModelError(string.Empty, "Merci de confirmer que vous n'êtes pas un robot.");
                return View(model);
            }

            var corpsEmail = EmailTemplates.MessageContact(model.Nom, model.Email, model.Message);
            await _emailService.EnvoyerAsync(EmailDestinataireContact, $"Nouveau message de {model.Nom}", corpsEmail);

            TempData["ContactEnvoye"] = true;
            return RedirectToAction(nameof(Contact));
        }

        // Canal de reclamation trace (Qualiopi indicateur 32), distinct du formulaire de
        // contact ci-dessus : chaque depot est persiste avec un statut audite plutot que
        // simplement relaye par email, cf. IReclamationService.
        [HttpGet]
        [Route("reclamation")]
        public IActionResult Reclamation()
        {
            return View(new ReclamationFormModel());
        }

        [HttpPost]
        [Route("reclamation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reclamation(ReclamationFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _reclamationService.DeposerAsync(new ReclamationInput
            {
                Nom = model.Nom,
                Email = model.Email,
                Message = model.Message,
                UtilisateurId = _userManager.GetUserId(User),
            });

            TempData["ReclamationEnvoyee"] = true;
            return RedirectToAction(nameof(Reclamation));
        }

        [HttpGet]
        [Route("formations")]
        public async Task<IActionResult> Formations()
        {
            var toutesLesCohortes = await _cohorteService.GetAllAsync();
            var nombreCohortesOuvertesParChallenge = toutesLesCohortes
                .Where(c => c.ChallengeMode == ModePlateforme.BtoC && c.Statut == StatutCohorte.EnPreparation)
                .GroupBy(c => c.ChallengeId)
                .ToDictionary(g => g.Key, g => g.Count());

            var challenges = await _challengeService.GetAllAsync();
            var challengesDisponibles = challenges
                .Where(c => c.Mode == ModePlateforme.BtoC && c.Statut == StatutChallenge.Publie)
                // "Matinale dirigeants" (SENS-04) est la carte la plus directement adressee a la
                // cible decideur PME/ETI du bloc d'accroche - mise en avant en toute premiere
                // position plutot que simplement triee avec le reste de Sensibiliser (prompt
                // 2e audit prospection).
                .OrderBy(c => c.Code == "SENS-04" ? 0 : 1)
                .ThenBy(c => RangSousFamille(c.SousFamille))
                .ThenBy(c => c.Titre)
                .Select(challenge => new FormationViewModel
                {
                    ChallengeId = challenge.Id,
                    Titre = challenge.Titre,
                    Slogan = challenge.Slogan,
                    DescriptionExtrait = ExtraireTexte(challenge.Description, 110),
                    NombreEtapes = challenge.NombreEtapes,
                    NombreCohortesOuvertes = nombreCohortesOuvertesParChallenge.GetValueOrDefault(challenge.Id),
                    Thematique = challenge.Thematique,
                    SousFamille = challenge.SousFamille,
                    Duree = challenge.Duree,
                    PublicVise = challenge.PublicVise,
                    Financement = challenge.Financement,
                    Prix = challenge.Prix,
                    DisponibleEnPresentiel = challenge.DisponibleEnPresentiel,
                })
                .ToList();

            return View(challengesDisponibles);
        }

        // Page de presentation dediee a un Challenge (bouton "Decouvrir ce Challenge" depuis
        // la liste /formations) : description complete, programme detaille etape par etape,
        // et toutes les Cohortes ouvertes a l'inscription pour ce Challenge.
        [HttpGet]
        [Route("formations/{challengeId:int}")]
        public async Task<IActionResult> Formation(int challengeId)
        {
            var challenge = await _challengeService.GetByIdAsync(challengeId);
            if (challenge is null || challenge.Mode != ModePlateforme.BtoC || challenge.Statut != StatutChallenge.Publie)
            {
                return NotFound();
            }

            var toutesLesCohortes = await _cohorteService.GetAllAsync();
            var cohortesOuvertes = toutesLesCohortes
                .Where(c => c.ChallengeId == challengeId && c.Statut == StatutCohorte.EnPreparation)
                .ToList();

            var viewModel = new FormationDetailViewModel
            {
                ChallengeId = challenge.Id,
                Titre = challenge.Titre,
                Slogan = challenge.Slogan,
                Description = challenge.Description,
                Thematique = challenge.Thematique,
                SousFamille = challenge.SousFamille,
                Duree = challenge.Duree,
                PublicVise = challenge.PublicVise,
                Financement = challenge.Financement,
                Prix = challenge.Prix,
                DisponibleEnPresentiel = challenge.DisponibleEnPresentiel,
                Prerequis = challenge.Prerequis,
                Etapes = challenge.Etapes
                    .OrderBy(e => e.NumeroEtape)
                    .Select(e => new EtapeApercuViewModel
                    {
                        NumeroEtape = e.NumeroEtape,
                        TitreEtape = e.TitreEtape,
                        ObjectifPedagogique = e.ObjectifPedagogique,
                    })
                    .ToList(),
                CohortesOuvertes = cohortesOuvertes,
            };

            return View("FormationDetail", viewModel);
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

            var cohorte = await _cohorteService.GetResumeAsync(cohorteId);
            var (success, errorMessage) = await _cohorteService.AutoInscrireAsync(cohorteId, userId);
            TempData["StatusMessage"] = success
                ? "Inscription enregistrée ! Retrouvez votre parcours depuis votre tableau de bord dès son lancement."
                : errorMessage;

            return cohorte is not null
                ? RedirectToAction(nameof(Formation), new { challengeId = cohorte.ChallengeId })
                : RedirectToAction(nameof(Formations));
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

            // Notifie le gestionnaire par email (cf. CLAUDE.md : aucune automatisation ne
            // doit se substituer a la validation humaine, mais encore faut-il qu'il soit
            // informe qu'une demande attend dans /Administration/Cohortes/Embarquement).
            if (success)
            {
                var demandeur = await _userManager.GetUserAsync(User);
                var challenge = await _challengeService.GetByIdAsync(challengeId);
                if (demandeur is not null && challenge is not null)
                {
                    var nomDemandeur = string.IsNullOrWhiteSpace(demandeur.Prenom) && string.IsNullOrWhiteSpace(demandeur.Nom)
                        ? demandeur.Email ?? "Utilisateur"
                        : $"{demandeur.Prenom} {demandeur.Nom}".Trim();
                    var corpsEmail = EmailTemplates.DemandeEmbarquement(nomDemandeur, demandeur.Email ?? "", challenge.Titre);
                    await _emailService.EnvoyerAsync(EmailDestinataireContact, $"Demande d'embarquement — {challenge.Titre}", corpsEmail);
                }
            }

            return RedirectToAction(nameof(Formation), new { challengeId });
        }

        // Ordre d'affichage du catalogue (prompt audit prospection : "tri qui suit les 4
        // etapes" plutot que l'ordre alphabetique, qui met en avant des sujets avances comme
        // l'IA avant les parcours d'entree de gamme). Suit le bloc "Par ou commencer ?" de
        // /formations : Sensibiliser -> Bilan -> trajectoire -> postes -> IA -> GRAINE, puis
        // les parcours Humain & Organisation.
        private static int RangSousFamille(SousFamilleChallenge? sousFamille) => sousFamille switch
        {
            SousFamilleChallenge.Sensibiliser => 1,
            SousFamilleChallenge.DuBilanALAction => 2,
            SousFamilleChallenge.PiloterTrajectoireSnbc => 3,
            SousFamilleChallenge.DefisParPosteEmission => 4,
            SousFamilleChallenge.IaEtTransition => 5,
            SousFamilleChallenge.ManagerEtEmbarquer => 6,
            SousFamilleChallenge.InspireesDuGraine => 7,
            SousFamilleChallenge.ParticuliersEtReconversion => 8,
            _ => 9,
        };

        // Extrait texte pour l'apercu carte (prompt : "on voit encore les balises HTML" sur
        // la liste) : la Description peut desormais contenir du HTML simple (paragraphes,
        // listes) rendu tel quel sur la page de detail - mais un extrait tronque au milieu
        // d'une balise casserait le rendu, donc on retire les balises avant de tronquer.
        private static string? ExtraireTexte(string? html, int longueurMax)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            var texte = System.Net.WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*>", " "));
            texte = Regex.Replace(texte, @"\s+", " ").Trim();

            return texte.Length > longueurMax ? texte[..longueurMax].TrimEnd() + "…" : texte;
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

    // Carte compacte du catalogue public /formations : juste de quoi decider si on va voir
    // le detail (extrait de description, nombre de sessions ouvertes) - le detail complet
    // (etapes, choix de la Cohorte, embarquement) vit sur FormationDetailViewModel.
    public class FormationViewModel
    {
        public int ChallengeId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Slogan { get; set; }
        public string? DescriptionExtrait { get; set; }
        public int NombreEtapes { get; set; }
        public int NombreCohortesOuvertes { get; set; }
        public ThematiqueChallenge Thematique { get; set; }
        public SousFamilleChallenge? SousFamille { get; set; }
        public string? Duree { get; set; }
        public string? PublicVise { get; set; }
        public string? Financement { get; set; }
        public string? Prix { get; set; }
        public bool DisponibleEnPresentiel { get; set; }
    }

    // Page de presentation dediee a un Challenge (prompt section H) : description complete
    // (rendue en HTML - cf. vue, la saisie admin peut contenir des balises simples),
    // programme etape par etape, et les Cohortes actuellement ouvertes a l'inscription pour
    // ce Challenge (jamais une Cohorte Proposee - cf. ICohorteService.GetAllAsync).
    public class FormationDetailViewModel
    {
        public int ChallengeId { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Slogan { get; set; }
        public string? Description { get; set; }
        public ThematiqueChallenge Thematique { get; set; }
        public SousFamilleChallenge? SousFamille { get; set; }
        public string? Duree { get; set; }
        public string? PublicVise { get; set; }
        public string? Financement { get; set; }
        public string? Prix { get; set; }
        public bool DisponibleEnPresentiel { get; set; }
        public string? Prerequis { get; set; }
        public List<EtapeApercuViewModel> Etapes { get; set; } = [];
        public List<CohorteResume> CohortesOuvertes { get; set; } = [];
    }

    public class EtapeApercuViewModel
    {
        public int NumeroEtape { get; set; }
        public string TitreEtape { get; set; } = string.Empty;
        public string? ObjectifPedagogique { get; set; }
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

    public class ReclamationFormModel
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

        [Required(ErrorMessage = "Merci de décrire votre réclamation.")]
        [StringLength(4000)]
        [Display(Name = "Votre réclamation")]
        public string Message { get; set; } = string.Empty;
    }
}
