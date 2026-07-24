using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class CarteCompetenceServiceTests
{
    private static CarteCompetenceInput NouvelleCarteInput(string code) => new()
    {
        Code = code,
        Niveau = NiveauCarte.Debutant,
        TitreTheorie = $"Titre {code}",
    };

    [Fact]
    public async Task CreateAsync_CreeUneCarte_QuandLeCodeEstUnique()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);

        var (success, errorMessage, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        Assert.True(success);
        Assert.Null(errorMessage);
        Assert.NotNull(carte);
        Assert.Equal("MAN-C23", carte!.Code);
    }

    [Fact]
    public async Task CreateAsync_Echoue_QuandLeCodeExisteDeja()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);

        await service.CreateAsync(NouvelleCarteInput("MAN-C23"));
        var (success, errorMessage, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(carte);
    }

    [Fact]
    public async Task RechercherAsync_FiltreParCodeEtParNiveau()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);

        await service.CreateAsync(NouvelleCarteInput("MAN-C23"));
        await service.CreateAsync(new CarteCompetenceInput { Code = "MAN-C15", Niveau = NiveauCarte.Expert, TitreTheorie = "OSBD" });
        await service.CreateAsync(new CarteCompetenceInput { Code = "COM-C01", Niveau = NiveauCarte.Debutant, TitreTheorie = "Écoute active" });

        var resultatParCode = await service.RechercherAsync(new CarteCompetenceFiltre { Recherche = "MAN-C23" });
        Assert.Equal(1, resultatParCode.NombreTotal);
        Assert.Equal("MAN-C23", resultatParCode.Cartes.Single().Code);

        var resultatParNiveau = await service.RechercherAsync(new CarteCompetenceFiltre { Niveau = NiveauCarte.Expert });
        Assert.Equal(1, resultatParNiveau.NombreTotal);
        Assert.Equal("MAN-C15", resultatParNiveau.Cartes.Single().Code);

        var resultatParTitre = await service.RechercherAsync(new CarteCompetenceFiltre { Recherche = "Écoute" });
        Assert.Equal(1, resultatParTitre.NombreTotal);
        Assert.Equal("COM-C01", resultatParTitre.Cartes.Single().Code);
    }

    [Fact]
    public async Task AttribuerAsync_CreeUneAttributionActiveEtTracable()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);

        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        var (success, errorMessage) = await service.AttribuerAsync([carte!.Id], [apprenant.Id], coach.Id, "Défi conflit d'équipe");

        Assert.True(success);
        Assert.Null(errorMessage);

        var attributions = await service.GetAttributionsPourCarteAsync(carte.Id);
        var attribution = Assert.Single(attributions);
        Assert.Equal(apprenant.Id, attribution.UtilisateurId);
        Assert.Equal(coach.Id, attribution.AttribueParId);
        Assert.True(attribution.EstActif);
        Assert.Equal("Défi conflit d'équipe", attribution.Contexte);
    }

    [Fact]
    public async Task DesattribuerAsync_DesactiveLAttribution_SansLaSupprimer()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);

        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));
        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        await service.AttribuerAsync([carte!.Id], [apprenant.Id], coach.Id, null);
        var attribution = (await service.GetAttributionsPourCarteAsync(carte.Id)).Single();

        var (success, _) = await service.DesattribuerAsync(attribution.Id);

        Assert.True(success);
        var attributionsActives = (await service.GetAttributionsPourCarteAsync(carte.Id)).Where(a => a.EstActif);
        Assert.Empty(attributionsActives);
    }
}
