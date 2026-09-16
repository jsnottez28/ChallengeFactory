using Application.Common.Interfaces;
using Web.Data;
using Web.Services;

namespace Integration.TestSupport;

// Depuis l'ajout du garde-fou "tous les emargements doivent etre signes avant de valider
// une etape" (cf. CohorteService.TousLesEmargementsSontSignesAsync), tout test qui appelle
// ValiderEtapeAsync sur une etape ayant des cartes attribuees doit d'abord planifier une
// visio, envoyer les emargements et les faire signer par chaque membre - factorise ici
// plutot que duplique dans chaque classe de test.
internal static class EmargementTestHelper
{
    public static async Task SignerTousLesEmargementsEtapeCouranteAsync(
        ApplicationDbContext dbContext, IEmailService emailService, int cohorteId, string gestionnaireId, params ApplicationUser[] membres)
    {
        var visioService = new VisioService(dbContext, emailService);
        await visioService.PlanifierAsync(cohorteId, gestionnaireId, DateTime.UtcNow, "https://meet.test.local/seance");

        var emargementService = new EmargementService(dbContext, emailService, new FakePreuveFichierStockageService());
        await emargementService.EnvoyerEmargementsEtapeCouranteAsync(cohorteId, _ => "https://test.local/emargement");

        foreach (var membre in membres)
        {
            var aSigner = await emargementService.GetPourSignatureAsync(cohorteId, membre.Id);
            if (aSigner is null)
            {
                continue;
            }

            var emargementIds = aSigner.Cartes.Select(c => c.EmargementId).ToList();
            await emargementService.SignerAsync(cohorteId, membre.Id, emargementIds, null, null, [1, 2, 3]);
        }
    }
}
