using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class PreBilanCarboneServiceTests
{
    // Facteurs minimalistes mais representatifs des deux TypeDonnee - la seed reelle (29
    // facteurs, ratios ADEME Base Carbone) vit dans une migration SQL, jamais executee par
    // le provider InMemory : chaque test seme ici les quelques facteurs dont il a besoin.
    private static async Task<(FacteurEmission Achat, FacteurEmission Energie)> SemerFacteursAsync(ApplicationDbContext dbContext)
    {
        var achat = new FacteurEmission
        {
            Code = "ACHAT_IT",
            Poste = PosteEmission.AchatsBiensEtServices,
            TypeDonnee = TypeDonneeActivite.Monetaire,
            Nom = "Informatique, logiciels et hébergement",
            Unite = "€",
            ValeurKgCO2eParUnite = 0.075m,
            Source = "Programmation, conseil IT / Services d'information - 2023, France continentale, Base Carbone",
            Ordre = 1,
            Actif = true,
        };
        var energie = new FacteurEmission
        {
            Code = "ENERGIE_ELEC",
            Poste = PosteEmission.Energie,
            TypeDonnee = TypeDonneeActivite.Physique,
            Nom = "Électricité",
            Unite = "kWh",
            ValeurKgCO2eParUnite = 0.058m,
            Source = "Électricité achetée, mix moyen France - 2023, France continentale, Base Carbone",
            Ordre = 1,
            Actif = true,
        };
        dbContext.FacteursEmission.AddRange(achat, energie);
        await dbContext.SaveChangesAsync();
        return (achat, energie);
    }

    [Fact]
    public async Task GetFacteursDisponiblesAsync_NeRenvoieQueLesFacteursActifs()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (achat, _) = await SemerFacteursAsync(dbContext);
        dbContext.FacteursEmission.Add(new FacteurEmission
        {
            Code = "INACTIF",
            Poste = PosteEmission.AchatsBiensEtServices,
            TypeDonnee = TypeDonneeActivite.Monetaire,
            Nom = "Desactive",
            Unite = "€",
            ValeurKgCO2eParUnite = 1,
            Source = "test",
            Ordre = 99,
            Actif = false,
        });
        await dbContext.SaveChangesAsync();

        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());

        var facteurs = await service.GetFacteursDisponiblesAsync();

        Assert.Equal(2, facteurs.Count);
        Assert.DoesNotContain(facteurs, f => f.Code == "INACTIF");
        Assert.Contains(facteurs, f => f.Code == achat.Code);
    }

    [Fact]
    public async Task DeposerAsync_CalculeLesEmissionsEtEnregistreEnBase()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        await SemerFacteursAsync(dbContext);
        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await service.DeposerAsync(new PreBilanCarboneInput
        {
            Email = "prospect@test.local",
            Societe = "Test SAS",
            ValeursParCodeFacteur = new Dictionary<string, decimal>
            {
                ["ACHAT_IT"] = 10_000m, // 10 000 € * 0.075 kgCO2e/€ = 750 kgCO2e
                ["ENERGIE_ELEC"] = 20_000m, // 20 000 kWh * 0.058 kgCO2e/kWh = 1160 kgCO2e
            },
        });

        Assert.True(success, errorMessage);
        Assert.NotNull(resultat);
        Assert.Equal(1910m, resultat!.TotalEmissionsKgCO2e);
        Assert.Equal(2, resultat.Lignes.Count);
        Assert.Equal(750m, resultat.Lignes.Single(l => l.Code == "ACHAT_IT").EmissionsKgCO2e);
        Assert.Equal(1160m, resultat.Lignes.Single(l => l.Code == "ENERGIE_ELEC").EmissionsKgCO2e);
        Assert.Equal(2, resultat.ParPoste.Count);

        var enBase = await dbContext.PreBilansCarbone.Include(p => p.Lignes).SingleAsync();
        Assert.Equal("prospect@test.local", enBase.Email);
        Assert.Equal("Test SAS", enBase.Societe);
        Assert.Equal(1910m, enBase.TotalEmissionsKgCO2e);
        Assert.Equal(2, enBase.Lignes.Count);
        Assert.Equal(StatutPreBilanCrm.Nouveau, enBase.StatutCrm);
    }

    [Fact]
    public async Task DeposerAsync_IgnoreLesValeursNullesOuNegatives()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        await SemerFacteursAsync(dbContext);
        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await service.DeposerAsync(new PreBilanCarboneInput
        {
            Email = "prospect@test.local",
            ValeursParCodeFacteur = new Dictionary<string, decimal>
            {
                ["ACHAT_IT"] = 1_000m,
                ["ENERGIE_ELEC"] = 0m,
            },
        });

        Assert.True(success, errorMessage);
        Assert.Single(resultat!.Lignes);
        Assert.Equal("ACHAT_IT", resultat.Lignes[0].Code);
    }

    [Fact]
    public async Task DeposerAsync_Echoue_SiAucuneValeurRenseignee()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        await SemerFacteursAsync(dbContext);
        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await service.DeposerAsync(new PreBilanCarboneInput
        {
            Email = "prospect@test.local",
            ValeursParCodeFacteur = [],
        });

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
        Assert.False(await dbContext.PreBilansCarbone.AnyAsync());
    }

    [Fact]
    public async Task DeposerAsync_Echoue_SiEmailManquant()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        await SemerFacteursAsync(dbContext);
        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());

        var (success, errorMessage, resultat) = await service.DeposerAsync(new PreBilanCarboneInput
        {
            Email = "",
            ValeursParCodeFacteur = new Dictionary<string, decimal> { ["ACHAT_IT"] = 1000m },
        });

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(resultat);
    }

    [Fact]
    public async Task DeposerAsync_EnvoieUnEmailDeResultat()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        await SemerFacteursAsync(dbContext);
        var emailService = new FakeEmailService();
        var service = new PreBilanCarboneService(dbContext, emailService);

        await service.DeposerAsync(new PreBilanCarboneInput
        {
            Email = "prospect@test.local",
            ValeursParCodeFacteur = new Dictionary<string, decimal> { ["ACHAT_IT"] = 1000m },
        });

        var envoi = Assert.Single(emailService.Envois);
        Assert.Equal("prospect@test.local", envoi.Destinataire);
        Assert.Contains("carbone", envoi.Sujet, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangerStatutCrmAsync_MetAJourLeStatut()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        await SemerFacteursAsync(dbContext);
        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());
        var (_, _, resultat) = await service.DeposerAsync(new PreBilanCarboneInput
        {
            Email = "prospect@test.local",
            ValeursParCodeFacteur = new Dictionary<string, decimal> { ["ACHAT_IT"] = 1000m },
        });

        var (success, errorMessage) = await service.ChangerStatutCrmAsync(resultat!.Id, StatutPreBilanCrm.Qualifie);

        Assert.True(success, errorMessage);
        var tous = await service.GetTousAsync();
        Assert.Equal(StatutPreBilanCrm.Qualifie, tous.Single().StatutCrm);
    }

    [Fact]
    public async Task ChangerStatutCrmAsync_Echoue_SiIntrouvable()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());

        var (success, errorMessage) = await service.ChangerStatutCrmAsync(999, StatutPreBilanCrm.Qualifie);

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task GetResultatAsync_RenvoieNull_SiIntrouvable()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new PreBilanCarboneService(dbContext, new FakeEmailService());

        var resultat = await service.GetResultatAsync(999);

        Assert.Null(resultat);
    }
}
