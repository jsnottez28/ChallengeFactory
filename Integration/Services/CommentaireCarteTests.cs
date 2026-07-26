using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Web.Data;
using Web.Services;

namespace Integration.Services;

// Couvre le prompt "Notes personnelles sur les cartes" (section 2) - tests minimums
// explicitement demandes :
// - un commentaire n'est visible que par son auteur, meme un Gestionnaire ne le voit pas ;
// - un apprenant peut ajouter plusieurs commentaires successifs sans ecraser les
//   precedents, et editer/supprimer uniquement les siens.
public class CommentaireCarteTests
{
    private static async Task<(ApplicationDbContext DbContext, ICarteApprenantService ApprenantService, CarteCompetence Carte, ApplicationUser Apprenant, ApplicationUser AutreApprenant, ApplicationUser Coach)> PreparerAsync()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        var userManager = TestUserManagerFactory.Create(dbContext);
        var carteService = new CarteCompetenceService(dbContext);
        var apprenantService = new CarteApprenantService(dbContext, userManager);

        var (_, _, carte) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = "MAN-C23",
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "Conduire le changement",
        });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        var autreApprenant = new ApplicationUser { UserName = "autre@test.local", Email = "autre@test.local" };
        var coach = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local" };
        dbContext.Users.AddRange(apprenant, autreApprenant, coach);
        await dbContext.SaveChangesAsync();

        await carteService.AttribuerAsync([carte!.Id], [apprenant.Id, autreApprenant.Id], coach.Id, null);

        return (dbContext, apprenantService, carte, apprenant, autreApprenant, coach);
    }

    [Fact]
    public async Task AjouterCommentaireAsync_NEstVisibleQueParSonAuteur_MemeUnGestionnaireNeLeVoitPas()
    {
        var (dbContext, apprenantService, carte, apprenant, autreApprenant, coach) = await PreparerAsync();
        await using var _ = dbContext;

        var (success, errorMessage, _) = await apprenantService.AjouterCommentaireAsync(apprenant.Id, carte.Id, "Cette carte me parle beaucoup en ce moment.");
        Assert.True(success, errorMessage);

        var mesCommentaires = await apprenantService.GetMesCommentairesAsync(apprenant.Id, carte.Id);
        var commentaire = Assert.Single(mesCommentaires);
        Assert.Equal("Cette carte me parle beaucoup en ce moment.", commentaire.Contenu);

        // Un autre apprenant qui a pourtant AUSSI la carte attribuee ne voit rien.
        var commentairesAutreApprenant = await apprenantService.GetMesCommentairesAsync(autreApprenant.Id, carte.Id);
        Assert.Empty(commentairesAutreApprenant);

        // Meme un Gestionnaire n'a aucun moyen d'acceder aux notes d'un apprenant : le
        // service n'expose que GetMesCommentairesAsync(utilisateurId, carteId), scope par
        // construction sur l'utilisateur appelant - il n'existe pas de "vue Gestionnaire"
        // de ces notes. On verifie qu'interroger avec l'id du Coach ne renvoie rien non plus.
        var commentairesVusParCoach = await apprenantService.GetMesCommentairesAsync(coach.Id, carte.Id);
        Assert.Empty(commentairesVusParCoach);
    }

    [Fact]
    public async Task AjouterCommentaireAsync_PlusieursCommentairesSuccessifs_NEcrasentPasLesPrecedents()
    {
        var (dbContext, apprenantService, carte, apprenant, _, _) = await PreparerAsync();
        await using var _ = dbContext;

        await apprenantService.AjouterCommentaireAsync(apprenant.Id, carte.Id, "Premier passage sur cette carte.");
        await apprenantService.AjouterCommentaireAsync(apprenant.Id, carte.Id, "Deuxième passage, je progresse.");
        await apprenantService.AjouterCommentaireAsync(apprenant.Id, carte.Id, "Troisième passage, ça devient naturel.");

        var commentaires = await apprenantService.GetMesCommentairesAsync(apprenant.Id, carte.Id);

        Assert.Equal(3, commentaires.Count);
        Assert.Contains(commentaires, c => c.Contenu == "Premier passage sur cette carte.");
        Assert.Contains(commentaires, c => c.Contenu == "Deuxième passage, je progresse.");
        Assert.Contains(commentaires, c => c.Contenu == "Troisième passage, ça devient naturel.");
        // Ordre chronologique.
        Assert.True(commentaires.SequenceEqual(commentaires.OrderBy(c => c.DateCreation)));
    }

    [Fact]
    public async Task ModifierCommentaireAsync_ReussitPourSonPropreCommentaire_EtHorodateLaModification()
    {
        var (dbContext, apprenantService, carte, apprenant, _, _) = await PreparerAsync();
        await using var _ = dbContext;

        var (_, _, commentaireId) = await apprenantService.AjouterCommentaireAsync(apprenant.Id, carte.Id, "Version initiale.");

        var (success, errorMessage) = await apprenantService.ModifierCommentaireAsync(commentaireId!.Value, apprenant.Id, "Version corrigée.");

        Assert.True(success, errorMessage);

        var commentaires = await apprenantService.GetMesCommentairesAsync(apprenant.Id, carte.Id);
        var commentaire = Assert.Single(commentaires);
        Assert.Equal("Version corrigée.", commentaire.Contenu);
        Assert.NotNull(commentaire.DateModification);
    }

    [Fact]
    public async Task ModifierCommentaireAsync_EchoueSiCeNEstPasSonPropreCommentaire()
    {
        var (dbContext, apprenantService, carte, apprenant, autreApprenant, _) = await PreparerAsync();
        await using var _ = dbContext;

        var (_, _, commentaireId) = await apprenantService.AjouterCommentaireAsync(apprenant.Id, carte.Id, "Note privée de l'apprenant.");

        var (success, errorMessage) = await apprenantService.ModifierCommentaireAsync(commentaireId!.Value, autreApprenant.Id, "Tentative de modification par un intrus.");

        Assert.False(success);
        Assert.NotNull(errorMessage);

        // Le contenu original n'a pas bouge.
        var commentaires = await apprenantService.GetMesCommentairesAsync(apprenant.Id, carte.Id);
        Assert.Equal("Note privée de l'apprenant.", Assert.Single(commentaires).Contenu);
    }

    [Fact]
    public async Task SupprimerCommentaireAsync_ReussitPourSonPropreCommentaire_EchoueSinon()
    {
        var (dbContext, apprenantService, carte, apprenant, autreApprenant, _) = await PreparerAsync();
        await using var _ = dbContext;

        var (_, _, commentaireId) = await apprenantService.AjouterCommentaireAsync(apprenant.Id, carte.Id, "À supprimer.");

        var (echecIntrus, erreurIntrus) = await apprenantService.SupprimerCommentaireAsync(commentaireId!.Value, autreApprenant.Id);
        Assert.False(echecIntrus);
        Assert.NotNull(erreurIntrus);
        Assert.Single(await apprenantService.GetMesCommentairesAsync(apprenant.Id, carte.Id));

        var (success, errorMessage) = await apprenantService.SupprimerCommentaireAsync(commentaireId.Value, apprenant.Id);
        Assert.True(success, errorMessage);
        Assert.Empty(await apprenantService.GetMesCommentairesAsync(apprenant.Id, carte.Id));
    }

    [Fact]
    public async Task AjouterCommentaireAsync_EchoueSurUneCarteNonAttribuee()
    {
        var dbContext = InMemoryDbContextFactory.Create();
        await using var _ = dbContext;
        var userManager = TestUserManagerFactory.Create(dbContext);
        var carteService = new CarteCompetenceService(dbContext);
        var apprenantService = new CarteApprenantService(dbContext, userManager);

        var (_, _, carteNonAttribuee) = await carteService.CreateAsync(new CarteCompetenceInput
        {
            Code = "MAN-C23",
            Niveau = NiveauCarte.Debutant,
            TitreTheorie = "Conduire le changement",
        });

        var apprenant = new ApplicationUser { UserName = "apprenant@test.local", Email = "apprenant@test.local" };
        dbContext.Users.Add(apprenant);
        await dbContext.SaveChangesAsync();

        var (success, errorMessage, commentaireId) = await apprenantService.AjouterCommentaireAsync(apprenant.Id, carteNonAttribuee!.Id, "Je tente quand même.");

        Assert.False(success);
        Assert.NotNull(errorMessage);
        Assert.Null(commentaireId);
    }
}
