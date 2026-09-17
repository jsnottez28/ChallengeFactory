using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test psychotechnique RIASEC a choix force par paires, en 2 rounds - accessible a tout
// utilisateur connecte via un lien fixe (copie-colle manuellement dans le champ "Defi
// individuel" d'une etape, ou envoye manuellement), pas rattache a une Cohorte/etape
// particuliere - cf. IRiasecService.
//
// Flux : OnGetAsync affiche le round 1 (36 paires). OnPostRound1Async verifie s'il faut un
// round 2 de departage (cf. IRiasecService.PreparerRound2) ; si oui, reaffiche la page avec
// les paires de departage et les reponses du round 1 portees en champs caches ; sinon
// finalise directement. OnPostRound2Async recoit le round 1 (champs caches) + le round 2 et
// finalise.
//
// Les reponses sont lues directement depuis Request.Form (prefixe "pair_"/"pair2_" +
// PaireId) plutot que via [BindProperty] sur un Dictionary<int,int> : le binder de
// dictionnaire d'ASP.NET Core s'est avere instable sur ce flux a deux formulaires avec
// report de valeurs en champs caches (FormatException sur le nom de propriete lui-meme).
// Lecture manuelle, simple et explicite - le meme principe que Niveaux[...] ailleurs sur la
// plateforme (TestPositionnement) fonctionne en formulaire unique, mais pas ici.
[Authorize]
public class TestRiasecModel(IRiasecService riasecService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Repasser { get; set; }

    // 1 = round 1 a afficher, 2 = round 2 a afficher (round 1 deja rempli, porte en champs
    // caches par la vue).
    public int Etape { get; private set; } = 1;

    public List<RiasecPaireInfo> PairesRound1 { get; private set; } = [];
    public List<RiasecPaireInfo> PairesRound2 { get; private set; } = [];

    public Dictionary<int, int> ReponsesRound1Portees { get; private set; } = [];

    public RiasecResultatInfo? DernierResultat { get; private set; }

    public string? StatusMessage { get; private set; }

    public bool DernierEnvoiReussi { get; private set; }

    private bool echecSoumission;

    public bool AfficherQuestionnaire => DernierResultat is null || Repasser || echecSoumission;

    public async Task OnGetAsync()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        DernierResultat = await riasecService.GetDernierResultatAsync(utilisateurId);

        if (AfficherQuestionnaire)
        {
            PairesRound1 = riasecService.GetPairesRound1();
        }
    }

    public async Task<IActionResult> OnPostRound1Async()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = LireReponses(pairesRound1, "pair_");

        var round2 = riasecService.PreparerRound2(reponsesRound1);

        if (round2 is null)
        {
            StatusMessage = "Merci de répondre à toutes les paires avant de continuer.";
            DernierEnvoiReussi = false;
            PairesRound1 = pairesRound1;
            return Page();
        }

        if (round2.Paires.Count == 0)
        {
            return await FinaliserAsync(utilisateurId, reponsesRound1, []);
        }

        Etape = 2;
        PairesRound2 = round2.Paires;
        ReponsesRound1Portees = reponsesRound1;
        return Page();
    }

    public async Task<IActionResult> OnPostRound2Async()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = LireReponses(pairesRound1, "pair_");

        var round2 = riasecService.PreparerRound2(reponsesRound1);
        var pairesRound2 = round2?.Paires ?? [];
        var reponsesRound2 = LireReponses(pairesRound2, "pair2_");

        return await FinaliserAsync(utilisateurId, reponsesRound1, reponsesRound2);
    }

    private Dictionary<int, int> LireReponses(List<RiasecPaireInfo> paires, string prefixe)
    {
        var reponses = new Dictionary<int, int>();
        foreach (var paire in paires)
        {
            var valeur = Request.Form[prefixe + paire.PaireId];
            if (valeur.Count > 0 && int.TryParse(valeur[0], out var choix))
            {
                reponses[paire.PaireId] = choix;
            }
        }
        return reponses;
    }

    private async Task<IActionResult> FinaliserAsync(string utilisateurId, Dictionary<int, int> reponsesRound1, Dictionary<int, int> reponsesRound2)
    {
        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateurId, reponsesRound1, reponsesRound2);

        DernierEnvoiReussi = success;
        echecSoumission = !success;
        StatusMessage = success ? "Merci, votre test est terminé. Votre profil est ci-dessous et vous a été envoyé par email." : errorMessage;
        DernierResultat = success ? resultat : await riasecService.GetDernierResultatAsync(utilisateurId);

        if (!success)
        {
            Etape = 1;
            PairesRound1 = riasecService.GetPairesRound1();
        }

        return Page();
    }
}
