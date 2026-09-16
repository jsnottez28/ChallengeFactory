using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.TestSupport;

// Deux garde-fous bloquent desormais ValiderEtapeAsync : tous les emargements de l'etape
// doivent etre signes (cf. CohorteService.TousLesEmargementsSontSignesAsync), et - a
// l'etape 1 et/ou a la derniere etape - le test de connaissances amont/aval requis doit
// etre complet (cf. CohorteService.GetTestRequisManquantAsync). Tout test qui appelle
// ValiderEtapeAsync sur une etape ayant des cartes attribuees doit d'abord satisfaire les
// deux - factorise ici plutot que duplique dans chaque classe de test.
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
            await emargementService.SignerAsync(cohorteId, membre.Id, emargementIds, 1m, 1m, [1, 2, 3]);
        }

        await RepondreTestPositionnementRequisAsync(dbContext, emailService, cohorteId, gestionnaireId, membres);
    }

    // A l'etape 1, complete le test amont ; a la derniere etape, complete le test aval - les
    // deux si l'etape 1 est aussi la derniere (Challenge a une seule etape), car
    // CohorteService.GetTestRequisManquantAsync exige alors les deux independamment. Ne fait
    // rien sur les etapes intermediaires (aucun test requis) ni si le Challenge n'a aucune
    // carte (rien a evaluer, le service refuserait l'envoi).
    public static async Task RepondreTestPositionnementRequisAsync(
        ApplicationDbContext dbContext, IEmailService emailService, int cohorteId, string gestionnaireId, params ApplicationUser[] membres)
    {
        var cohorte = await dbContext.Cohortes.Include(c => c.Challenge).FirstAsync(c => c.Id == cohorteId);
        var aDesCartes = await dbContext.ChallengeEtapeCartes.AnyAsync(ec => ec.ChallengeEtape.ChallengeId == cohorte.ChallengeId);
        if (!aDesCartes)
        {
            return;
        }

        var typesRequis = new List<TypeTestPositionnement>();
        if (cohorte.EtapeCourante == 1)
        {
            typesRequis.Add(TypeTestPositionnement.Amont);
        }

        if (cohorte.EtapeCourante == cohorte.Challenge.NombreEtapes)
        {
            typesRequis.Add(TypeTestPositionnement.Aval);
        }

        var testPositionnementService = new TestPositionnementService(dbContext, emailService);

        foreach (var type in typesRequis)
        {
            var (envoiSuccess, envoiErreur) = await testPositionnementService.EnvoyerAsync(cohorteId, type, gestionnaireId, _ => "https://test.local/test-positionnement");
            if (!envoiSuccess)
            {
                throw new InvalidOperationException($"EnvoyerAsync a echoue pour {type} : {envoiErreur}");
            }

            foreach (var membre in membres)
            {
                var info = await testPositionnementService.GetPourReponseAsync(cohorteId, type, membre.Id);
                if (info is null)
                {
                    throw new InvalidOperationException($"GetPourReponseAsync a renvoye null pour {type}");
                }

                var niveaux = info.Cartes.ToDictionary(c => c.CarteCompetenceId, _ => 5);
                var (repondreSuccess, repondreErreur) = await testPositionnementService.RepondreAsync(cohorteId, type, membre.Id, niveaux);
                if (!repondreSuccess)
                {
                    throw new InvalidOperationException($"RepondreAsync a echoue pour {type} : {repondreErreur}");
                }
            }
        }
    }
}
