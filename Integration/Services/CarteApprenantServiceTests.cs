using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class CarteApprenantServiceTests
{
    [Fact]
    public async Task GetCarteAttribueeAsync_RenvoieLaCarte_QuandElleEstAttribueeALUtilisateur()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var carteService = new CarteCompetenceService(dbContext);
        var apprenantService = new CarteApprenantService(dbContext, TestUserManagerFactory.Create(dbContext));

        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = "MAN-C23",
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "Conduire le changement",
        });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        await carteService.AttribuerAsync([carte!.Id], [apprenant.Id], coach.Id, null);

        var carteRecue = await apprenantService.GetCarteAttribueeAsync(apprenant.Id, carte.Id);

        Assert.NotNull(carteRecue);
        Assert.Equal(carte.Id, carteRecue!.Id);
    }

    [Fact]
    public async Task GetCarteAttribueeAsync_RenvoieNull_QuandLaCarteNEstPasAttribuee_MemeEnDevinantLId()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var carteService = new CarteCompetenceService(dbContext);
        var apprenantService = new CarteApprenantService(dbContext, TestUserManagerFactory.Create(dbContext));

        var (_, _, carteNonAttribuee) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = "MAN-C23",
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "Conduire le changement",
        });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        // Aucune attribution n'a ete creee : meme en forcant l'Id reel de la carte dans
        // l'URL, l'apprenant ne doit rien recevoir.
        var carteRecue = await apprenantService.GetCarteAttribueeAsync(apprenant.Id, carteNonAttribuee!.Id);

        Assert.Null(carteRecue);
    }

    [Fact]
    public async Task GetCarteAttribueeAsync_RenvoieNull_QuandLaCarteEstAttribueeAUnAutreUtilisateur()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var carteService = new CarteCompetenceService(dbContext);
        var apprenantService = new CarteApprenantService(dbContext, TestUserManagerFactory.Create(dbContext));

        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = "MAN-C23",
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "Conduire le changement",
        });

        var apprenantAutorise = new ApplicationUser { UserName = "autorise@test.local", Email = "autorise@test.local" };
        var apprenantIntrus = new ApplicationUser { UserName = "intrus@test.local", Email = "intrus@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenantAutorise, apprenantIntrus, coach);
        await dbContext.SaveChangesAsync();

        await carteService.AttribuerAsync([carte!.Id], [apprenantAutorise.Id], coach.Id, null);

        var carteRecueParIntrus = await apprenantService.GetCarteAttribueeAsync(apprenantIntrus.Id, carte.Id);

        Assert.Null(carteRecueParIntrus);
    }

    [Fact]
    public async Task GetCarteAttribueeAsync_RenvoieNull_ApresDesattribution()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var carteService = new CarteCompetenceService(dbContext);
        var apprenantService = new CarteApprenantService(dbContext, TestUserManagerFactory.Create(dbContext));

        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = "MAN-C23",
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "Conduire le changement",
        });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        await carteService.AttribuerAsync([carte!.Id], [apprenant.Id], coach.Id, null);
        var attribution = (await carteService.GetAttributionsPourCarteAsync(carte!.Id)).Single();
        await carteService.DesattribuerAsync(attribution.Id);

        var carteRecue = await apprenantService.GetCarteAttribueeAsync(apprenant.Id, carte.Id);

        Assert.Null(carteRecue);
    }

    [Fact]
    public async Task GetMesCartesAsync_NeRenvoieQueLesCartesAttribueesActives()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var carteService = new CarteCompetenceService(dbContext);
        var apprenantService = new CarteApprenantService(dbContext, TestUserManagerFactory.Create(dbContext));

        var (_, _, carteAttribuee) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "MAN-C23", Niveau = NiveauCarte.Debutant, TitreTheorie = "Conduire le changement" });
        var (_, _, carteNonAttribuee) = await carteService.CreateAsync(new CarteCompetenceInput { Code = "MAN-C15", Niveau = NiveauCarte.Expert, TitreTheorie = "OSBD" });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, coach);
        await dbContext.SaveChangesAsync();

        await carteService.AttribuerAsync([carteAttribuee!.Id], [apprenant.Id], coach.Id, null);

        var mesCartes = await apprenantService.GetMesCartesAsync(apprenant.Id);

        var carteRecue = Assert.Single(mesCartes);
        Assert.Equal(carteAttribuee.Id, carteRecue.Carte.Id);
        Assert.DoesNotContain(mesCartes, c => c.Carte.Id == carteNonAttribuee!.Id);
    }
}
