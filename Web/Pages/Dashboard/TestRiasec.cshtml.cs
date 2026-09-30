using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Data;

namespace Web.Pages.Dashboard;

// Test psychotechnique RIASEC a choix force par paires, en 2 ou 3 rounds - accessible a
// tout utilisateur connecte via un lien fixe (copie-colle manuellement dans le champ
// "Defi individuel" d'une etape, ou envoye manuellement), pas rattache a une
// Cohorte/etape particuliere - cf. IRiasecService.
//
// Flux : OnGetAsync affiche le round 1 (30 paires). OnPostRound1Async construit le round
// de fiabilite (cf. IRiasecService.PreparerFiabilite, toujours 6 paires, jamais
// conditionnel) et reaffiche la page avec ces paires + le round 1 porte en champs caches.
// OnPostFiabiliteAsync recoit round 1 + fiabilite, verifie s'il faut un round 2 de
// departage (cf. IRiasecService.PreparerRound2) ; si oui, reaffiche avec les paires de
// departage et round 1 + fiabilite portes en champs caches ; sinon finalise directement.
// OnPostRound2Async recoit round 1 + fiabilite (champs caches) + round 2 et finalise.
//
// Les reponses sont lues directement depuis Request.Form (prefixe "pair_"/"fiab_"/"pair2_"
// + PaireId) plutot que via [BindProperty] sur un Dictionary<int,int> : le binder de
// dictionnaire d'ASP.NET Core s'est avere instable sur ce flux a plusieurs formulaires avec
// report de valeurs en champs caches (FormatException sur le nom de propriete lui-meme).
// Lecture manuelle, simple et explicite - le meme principe que Niveaux[...] ailleurs sur la
// plateforme (TestPositionnement) fonctionne en formulaire unique, mais pas ici.
[Authorize]
public class TestRiasecModel(IRiasecService riasecService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Repasser { get; set; }

    // 1 = round 1, 2 = round de fiabilite (toujours affiche apres le round 1), 3 = round 2
    // de departage (conditionnel) - les etapes precedentes sont portees en champs caches
    // par la vue.
    public int Etape { get; private set; } = 1;

    public List<RiasecPaireInfo> PairesRound1 { get; private set; } = [];
    public List<RiasecPaireInfo> PairesFiabilite { get; private set; } = [];
    public List<RiasecPaireInfo> PairesRound2 { get; private set; } = [];

    public Dictionary<int, int> ReponsesRound1Portees { get; private set; } = [];
    public Dictionary<int, int> ReponsesFiabilitePortees { get; private set; } = [];

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

        var fiabilite = riasecService.PreparerFiabilite(reponsesRound1);

        if (fiabilite is null)
        {
            StatusMessage = "Merci de répondre à toutes les paires avant de continuer.";
            DernierEnvoiReussi = false;
            PairesRound1 = pairesRound1;
            return Page();
        }

        Etape = 2;
        PairesFiabilite = fiabilite;
        ReponsesRound1Portees = reponsesRound1;
        return Page();
    }

    public async Task<IActionResult> OnPostFiabiliteAsync()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = LireReponses(pairesRound1, "pair_");

        var pairesFiabilite = riasecService.PreparerFiabilite(reponsesRound1) ?? [];
        var reponsesFiabilite = LireReponses(pairesFiabilite, "fiab_");

        var round2 = riasecService.PreparerRound2(reponsesRound1);

        if (round2 is null)
        {
            StatusMessage = "Merci de répondre à toutes les paires avant de continuer.";
            DernierEnvoiReussi = false;
            PairesRound1 = riasecService.GetPairesRound1();
            return Page();
        }

        if (round2.Paires.Count == 0)
        {
            return await FinaliserAsync(utilisateurId, reponsesRound1, reponsesFiabilite, []);
        }

        Etape = 3;
        PairesRound2 = round2.Paires;
        ReponsesRound1Portees = reponsesRound1;
        ReponsesFiabilitePortees = reponsesFiabilite;
        return Page();
    }

    public async Task<IActionResult> OnPostRound2Async()
    {
        var utilisateurId = userManager.GetUserId(User)!;
        var pairesRound1 = riasecService.GetPairesRound1();
        var reponsesRound1 = LireReponses(pairesRound1, "pair_");

        var pairesFiabilite = riasecService.PreparerFiabilite(reponsesRound1) ?? [];
        var reponsesFiabilite = LireReponses(pairesFiabilite, "fiab_");

        var round2 = riasecService.PreparerRound2(reponsesRound1);
        var pairesRound2 = round2?.Paires ?? [];
        var reponsesRound2 = LireReponses(pairesRound2, "pair2_");

        return await FinaliserAsync(utilisateurId, reponsesRound1, reponsesFiabilite, reponsesRound2);
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

    private async Task<IActionResult> FinaliserAsync(string utilisateurId, Dictionary<int, int> reponsesRound1, Dictionary<int, int> reponsesFiabilite, Dictionary<int, int> reponsesRound2)
    {
        var (success, errorMessage, resultat) = await riasecService.RepondreAsync(utilisateurId, reponsesRound1, reponsesFiabilite, reponsesRound2);

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
