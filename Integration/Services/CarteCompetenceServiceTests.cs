using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task AjouterRessourceAsync_AjouteUnLienWeb()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        var (success, errorMessage) = await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput
        {
            Type = TypeRessourceCarte.LienWeb,
            Titre = "Article sur l'écoute active",
            Url = "https://example.com/ecoute-active",
        });

        Assert.True(success, errorMessage);
        var carteRechargee = await service.GetByIdAsync(carte.Id);
        var ressource = Assert.Single(carteRechargee!.Ressources);
        Assert.Equal(TypeRessourceCarte.LienWeb, ressource.Type);
        Assert.Equal("https://example.com/ecoute-active", ressource.Url);
        Assert.Null(ressource.NomFichier);
        Assert.Null(ressource.ReferenceTexte);
    }

    [Fact]
    public async Task AjouterRessourceAsync_AjouteUnFichierPdf()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        var (success, errorMessage) = await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput
        {
            Type = TypeRessourceCarte.FichierPdf,
            Titre = "Fiche technique OSBD",
            NomFichier = "abc123.pdf",
        });

        Assert.True(success, errorMessage);
        var carteRechargee = await service.GetByIdAsync(carte.Id);
        var ressource = Assert.Single(carteRechargee!.Ressources);
        Assert.Equal("abc123.pdf", ressource.NomFichier);
        Assert.Null(ressource.Url);
    }

    [Fact]
    public async Task AjouterRessourceAsync_AjouteUneReferenceBibliographique()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        var (success, errorMessage) = await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput
        {
            Type = TypeRessourceCarte.ReferenceBibliographique,
            Titre = "Rosenberg, la CNV",
            ReferenceTexte = "Rosenberg, M. (1999). Les mots sont des fenêtres (ou bien ce sont des murs). La Découverte.",
        });

        Assert.True(success, errorMessage);
        var carteRechargee = await service.GetByIdAsync(carte.Id);
        var ressource = Assert.Single(carteRechargee!.Ressources);
        Assert.Contains("Rosenberg", ressource.ReferenceTexte);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AjouterRessourceAsync_Echoue_SiLeTitreEstVide(string titre)
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        var (success, errorMessage) = await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput
        {
            Type = TypeRessourceCarte.LienWeb,
            Titre = titre,
            Url = "https://example.com",
        });

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task AjouterRessourceAsync_Echoue_SiLeChampAttenduPourLeTypeEstVide()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        // Type LienWeb sans Url renseignee.
        var (success, errorMessage) = await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput
        {
            Type = TypeRessourceCarte.LienWeb,
            Titre = "Lien sans URL",
        });

        Assert.False(success);
        Assert.NotNull(errorMessage);
        var carteRechargee = await service.GetByIdAsync(carte.Id);
        Assert.Empty(carteRechargee!.Ressources);
    }

    [Fact]
    public async Task AjouterRessourceAsync_Echoue_SiLaCarteNexistePas()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);

        var (success, errorMessage) = await service.AjouterRessourceAsync(999, new CarteRessourceInput
        {
            Type = TypeRessourceCarte.LienWeb,
            Titre = "Lien",
            Url = "https://example.com",
        });

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task AjouterRessourceAsync_IncrementeLordreAChaqueAjout()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));

        await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput { Type = TypeRessourceCarte.LienWeb, Titre = "Premier", Url = "https://example.com/1" });
        await service.AjouterRessourceAsync(carte.Id, new CarteRessourceInput { Type = TypeRessourceCarte.LienWeb, Titre = "Second", Url = "https://example.com/2" });

        var carteRechargee = await service.GetByIdAsync(carte.Id);
        var ressources = carteRechargee!.Ressources;
        Assert.Equal(2, ressources.Count);
        Assert.Equal("Premier", ressources[0].Titre);
        Assert.Equal("Second", ressources[1].Titre);
        Assert.Equal(0, ressources[0].Ordre);
        Assert.Equal(1, ressources[1].Ordre);
    }

    [Fact]
    public async Task SupprimerRessourceAsync_RetireLaRessource()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));
        await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput { Type = TypeRessourceCarte.LienWeb, Titre = "Lien", Url = "https://example.com" });
        var ressourceId = (await service.GetByIdAsync(carte.Id))!.Ressources.Single().Id;

        var (success, errorMessage) = await service.SupprimerRessourceAsync(ressourceId);

        Assert.True(success, errorMessage);
        var carteRechargee = await service.GetByIdAsync(carte.Id);
        Assert.Empty(carteRechargee!.Ressources);
    }

    [Fact]
    public async Task SupprimerRessourceAsync_Echoue_SiLaRessourceNexistePas()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);

        var (success, errorMessage) = await service.SupprimerRessourceAsync(999);

        Assert.False(success);
        Assert.NotNull(errorMessage);
    }

    [Fact]
    public async Task DeleteAsync_SupprimeEgalementLesRessourcesLiees()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new CarteCompetenceService(dbContext);
        var (_, _, carte) = await service.CreateAsync(NouvelleCarteInput("MAN-C23"));
        await service.AjouterRessourceAsync(carte!.Id, new CarteRessourceInput { Type = TypeRessourceCarte.LienWeb, Titre = "Lien", Url = "https://example.com" });

        var (success, errorMessage) = await service.DeleteAsync(carte.Id);

        Assert.True(success, errorMessage);
        Assert.False(await dbContext.CarteRessources.AnyAsync());
    }
}
