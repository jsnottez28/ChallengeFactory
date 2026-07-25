using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Services;

namespace Integration.Services;

public class ChallengeServiceTests
{
    [Fact]
    public async Task CreerChallengeEtapesEtCartesRattachees_FonctionneDeBoutEnBout()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (success, _, challenge) = await challengeService.CreateAsync(new ChallengeInput
        {
            Titre = "Management du changement",
            NombreEtapes = 2,
            Mode = ModePlateforme.BtoB,
        });
        Assert.True(success);

        var (etapeSuccess, _, etape1) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput
        {
            TitreEtape = "Recadrage sur les faits",
            DefiIndividuel = "Observer une situation et noter les faits.",
        });
        Assert.True(etapeSuccess);

        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = "MAN-C23",
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "OSBD",
        });

        var (attachSuccess, _) = await challengeService.DefinirCartesEtapeAsync(etape1!.Id, [carte!.Id]);
        Assert.True(attachSuccess);

        var challengeComplet = await challengeService.GetByIdAsync(challenge.Id);

        var etapeRecue = Assert.Single(challengeComplet!.Etapes);
        Assert.Equal(1, etapeRecue.NumeroEtape);
        Assert.Equal("Recadrage sur les faits", etapeRecue.TitreEtape);

        var carteRattachee = Assert.Single(etapeRecue.Cartes);
        Assert.Equal(carte.Id, carteRattachee.CarteCompetenceId);
    }

    [Fact]
    public async Task PublierAsync_Echoue_SiAucuneEtape()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Sans étape", Mode = ModePlateforme.BtoC });

        var (success, errorMessage) = await challengeService.PublierAsync(challenge!.Id);

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task PublierAsync_Reussit_AvecAuMoinsUneEtape()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Avec étape", Mode = ModePlateforme.BtoC });
        await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1" });

        var (success, _) = await challengeService.PublierAsync(challenge.Id);

        Assert.True(success);

        var challengeRecu = await challengeService.GetByIdAsync(challenge.Id);
        Assert.Equal(StatutChallenge.Publie, challengeRecu!.Statut);
    }

    [Fact]
    public async Task SupprimerAsync_Reussit_SiChallengeEnBrouillon_EtSupprimeAussiSesEtapesEtCartesRattachees()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);
        var carteService = new CarteCompetenceService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "À supprimer", Mode = ModePlateforme.BtoC });
        var (_, _, etape) = await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1" });
        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "SUP-1", Niveau = NiveauCarte.Debutant, TitreTheorie = "Carte" });
        await challengeService.DefinirCartesEtapeAsync(etape!.Id, [carte!.Id]);

        var (success, errorMessage) = await challengeService.SupprimerAsync(challenge.Id);

        Assert.True(success, errorMessage);
        Assert.Null(await challengeService.GetByIdAsync(challenge.Id));
        Assert.Null(await challengeService.GetEtapeByIdAsync(etape.Id));
    }

    [Fact]
    public async Task SupprimerAsync_Echoue_SiChallengeDejaPublie()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var challengeService = new ChallengeService(dbContext);

        var (_, _, challenge) = await challengeService.CreateAsync(new ChallengeInput { Titre = "Publié", Mode = ModePlateforme.BtoC });
        await challengeService.CreerEtapeAsync(challenge!.Id, new ChallengeEtapeInput { TitreEtape = "Étape 1" });
        await challengeService.PublierAsync(challenge.Id);

        var (success, errorMessage) = await challengeService.SupprimerAsync(challenge.Id);

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.NotNull(await challengeService.GetByIdAsync(challenge.Id));
    }
}
