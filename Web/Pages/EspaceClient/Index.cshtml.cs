using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;
using Web.Models;

namespace Web.Pages.EspaceClient;

// Accueil de l'espace client Bilan Carbone(R) : etat du dossier, acces au questionnaire de
// collecte, check-list des documents a rassembler une fois le questionnaire envoye. Les
// plans d'action de decarbonation viendront s'ajouter ici. Accessible a tout compte
// disposant d'un DossierCarbone (Participant ou ClientCarbone - les acces se cumulent,
// cf. Domain.Entities.TypeCompte), jamais filtre par TypeCompte seul.
[Authorize]
public class IndexModel(ICollecteCarboneService collecteService, UserManager<ApplicationUser> userManager) : PageModel
{
    private const long TailleMaxTableurOctets = 10 * 1024 * 1024; // 10 Mo

    public DossierCarboneInfo? Dossier { get; private set; }
    public List<ElementCollecteInfo> Checklist { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        Dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (Dossier is null)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        if (Dossier?.StatutQuestionnaire == StatutQuestionnaireCollecte.Soumis)
        {
            Checklist = await collecteService.GetChecklistAsync(Dossier.Id);
        }

        return Page();
    }

    // Self-service : le client telecharge directement son tableur de collecte, sans
    // attendre que le Gestionnaire le lui transmette manuellement. L'Id du Pre-diagnostic
    // vient toujours du dossier retrouve via l'utilisateur connecte, jamais d'une valeur
    // fournie par le client - meme principe que le reste de l'Espace Client.
    //
    // S'il a deja depose un fichier, on lui renvoie CE fichier (ses propres saisies en
    // cours), jamais une version fraichement regeneree - sinon retelecharger pour
    // completer au fur et a mesure lui ferait perdre tout ce qu'il a deja rempli.
    public async Task<IActionResult> OnGetTableurAsync()
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        var dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (dossier is null)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        if (dossier.TableurDeposeLe is not null)
        {
            var depot = await collecteService.TelechargerDernierTableurDeposeAsync(dossier.PreBilanCarboneId);
            if (depot is not null)
            {
                return File(depot.Value.Contenu, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", depot.Value.NomFichier);
            }
        }

        var lienEspaceClient = Url.Page("/EspaceClient/Index", null, null, Request.Scheme);
        var tableur = await collecteService.GenererTableurAsync(dossier.PreBilanCarboneId, lienEspaceClient);
        if (tableur is null)
        {
            return NotFound();
        }

        return File(tableur.Value.Contenu, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tableur.Value.NomFichier);
    }

    // Self-service : le client depose son tableur rempli (remplace le depot precedent -
    // un seul conserve). La "jauge" affichee ensuite est recalculee a partir de CE
    // fichier, pas saisie a la main par le client.
    public async Task<IActionResult> OnPostDeposerTableurAsync(IFormFile? fichier, bool collecteTerminee)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        if (fichier is null || fichier.Length == 0)
        {
            TempData["ErreurDepotTableur"] = "Merci de choisir un fichier avant de le déposer.";
            return RedirectToPage();
        }

        if (!string.Equals(Path.GetExtension(fichier.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErreurDepotTableur"] = "Seuls les fichiers .xlsx sont acceptés.";
            return RedirectToPage();
        }

        if (fichier.Length > TailleMaxTableurOctets)
        {
            TempData["ErreurDepotTableur"] = $"Ce fichier dépasse la taille maximale autorisée ({TailleMaxTableurOctets / (1024 * 1024)} Mo).";
            return RedirectToPage();
        }

        await using var flux = fichier.OpenReadStream();
        var (success, errorMessage) = await collecteService.DeposerTableurRempliAsync(utilisateur.Id, flux, fichier.FileName, collecteTerminee);

        TempData[success ? "StatusMessage" : "ErreurDepotTableur"] = success
            ? "Votre tableur a bien été déposé. L'avancement estimé a été mis à jour."
            : errorMessage;

        return RedirectToPage();
    }

    // Case a cocher par ligne de la check-list ("je l'ai déjà saisie dans mon fichier") -
    // purement declaratif, independant de la jauge calculee depuis le fichier depose.
    // elementsCoches : uniquement les cles des cases QUI SONT cochees (une case
    // decochee ne poste rien, comportement HTML standard) - remplace toute la selection.
    public async Task<IActionResult> OnPostCocherElementsAsync(List<string>? elementsCoches)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        await collecteService.EnregistrerElementsCochesAsync(utilisateur.Id, elementsCoches ?? []);

        return RedirectToPage();
    }

    // Mode simple (par defaut) : saisie directe dans le formulaire web, sans passer par le
    // tableur Excel - cf. ICollecteCarboneService.EnregistrerValeursFormulaireAsync.
    public async Task<IActionResult> OnPostEnregistrerValeursAsync(List<ValeurSaisieLigneForm>? lignes)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        var donnees = (lignes ?? []).Select(l => (l.Cle, new ValeurSaisieFichier
        {
            LibellePersonnalise = l.LibellePersonnalise,
            Valeur = l.Valeur,
            Unite = l.Unite,
            Type = l.Type,
            Source = l.Source,
            Hypotheses = l.Hypotheses,
            Statut = l.Statut,
            Commentaire = l.Commentaire,
        })).ToList();

        var nomAffiche = utilisateur.Prenom ?? utilisateur.Email ?? "Vous";
        var (success, errorMessage) = await collecteService.EnregistrerValeursFormulaireAsync(utilisateur.Id, nomAffiche, donnees);

        TempData[success ? "StatusMessage" : "ErreurDepotTableur"] = success ? "Vos données ont été enregistrées." : errorMessage;

        return RedirectToPage();
    }

    // Ajoute ou met a jour une ligne "libre" (donnee hors catalogue) pour un poste donne -
    // LigneLibreForm.Id vide = creation.
    public async Task<IActionResult> OnPostEnregistrerLigneLibreAsync(LigneLibreForm ligne)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        var nomAffiche = utilisateur.Prenom ?? utilisateur.Email ?? "Vous";
        var (success, errorMessage) = await collecteService.EnregistrerLigneLibreAsync(utilisateur.Id, nomAffiche, new LigneLibreCollecte
        {
            Id = ligne.Id ?? string.Empty,
            Poste = ligne.Poste,
            Donnee = ligne.Donnee,
            Valeur = ligne.Valeur,
            Unite = ligne.Unite,
            Type = ligne.Type,
            Source = ligne.Source,
            Hypotheses = ligne.Hypotheses,
            Statut = ligne.Statut,
            Commentaire = ligne.Commentaire,
        });

        TempData[success ? "StatusMessage" : "ErreurDepotTableur"] = success ? "Ligne enregistrée." : errorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSupprimerLigneLibreAsync(string ligneId)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        var (success, errorMessage) = await collecteService.SupprimerLigneLibreAsync(utilisateur.Id, ligneId);

        TempData[success ? "StatusMessage" : "ErreurDepotTableur"] = success ? "Ligne supprimée." : errorMessage;

        return RedirectToPage();
    }

    // Self-service : le contact principal invite un collegue a contribuer au meme dossier
    // (RH, facilities, comptabilite...). Acces complet, pas de perimetre par poste.
    public async Task<IActionResult> OnPostInviterCollaborateurAsync(string email)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        var dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (dossier is null)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        var (success, errorMessage) = await collecteService.InviterCollaborateurAsync(dossier.Id, email, utilisateur.Id, LienActivation, LienConnexion());

        TempData[success ? "StatusMessage" : "ErreurDepotTableur"] = success ? "Invitation envoyée." : errorMessage;

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRetirerCollaborateurAsync(string utilisateurId)
    {
        var utilisateur = await userManager.GetUserAsync(User);
        if (utilisateur is null)
        {
            return Challenge();
        }

        var dossier = await collecteService.GetDossierClientAsync(utilisateur.Id);
        if (dossier is null)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        var (success, errorMessage) = await collecteService.RetirerCollaborateurAsync(dossier.Id, utilisateurId);

        TempData[success ? "StatusMessage" : "ErreurDepotTableur"] = success ? "Accès retiré." : errorMessage;

        return RedirectToPage();
    }

    private string LienActivation(string token) =>
        Url.Page("/Compte/DefinirMotDePasse", null, new { token }, Request.Scheme) ?? $"/Compte/DefinirMotDePasse?token={token}";

    private string LienConnexion() =>
        Url.Page("/Account/Login", null, new { area = "Identity" }, Request.Scheme) ?? "/Identity/Account/Login";
}
