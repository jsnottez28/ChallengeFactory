using Application.Common.Interfaces;
using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class ReclamationServiceTests
{
    [Fact]
    public async Task DeposerAsync_PersisteLaReclamationEtEnvoieUnAccuseDeReception()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var emailService = new FakeEmailService();
        var reclamationService = new ReclamationService(dbContext, emailService);

        await reclamationService.DeposerAsync(new ReclamationInput
        {
            Nom = "Jeanne Dupont",
            Email = "jeanne@test.local",
            Message = "Le rythme de mon parcours est trop rapide.",
        });

        var reclamation = await dbContext.Reclamations.SingleAsync();
        Assert.Equal("Jeanne Dupont", reclamation.Nom);
        Assert.Equal(StatutReclamation.Recue, reclamation.Statut);
        Assert.Null(reclamation.UtilisateurId);

        var email = Assert.Single(emailService.Envois);
        Assert.Equal("jeanne@test.local", email.Destinataire);
        Assert.Contains("reçue", email.Sujet);
    }

    [Fact]
    public async Task DeposerAsync_RattacheLUtilisateurConnecteQuandFourni()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var reclamationService = new ReclamationService(dbContext, new FakeEmailService());

        var utilisateur = new ApplicationUser { UserName = "connecte@test.local", Email = "connecte@test.local" };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();

        await reclamationService.DeposerAsync(new ReclamationInput
        {
            Nom = "Utilisateur Connecté",
            Email = "connecte@test.local",
            Message = "Problème d'accès à ma carte.",
            UtilisateurId = utilisateur.Id,
        });

        var reclamation = await dbContext.Reclamations.SingleAsync();
        Assert.Equal(utilisateur.Id, reclamation.UtilisateurId);
    }

    [Fact]
    public async Task PrendreEnChargeAsync_PasseLeStatutAEnCours_MaisNEnvoieAucunEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var emailService = new FakeEmailService();
        var reclamationService = new ReclamationService(dbContext, emailService);

        await reclamationService.DeposerAsync(new ReclamationInput { Nom = "A", Email = "a@test.local", Message = "M" });
        var reclamation = await dbContext.Reclamations.SingleAsync();
        emailService.Envois.Clear();

        var (success, errorMessage) = await reclamationService.PrendreEnChargeAsync(reclamation.Id, "gestionnaire-id");

        Assert.True(success, errorMessage);
        var rechargee = await dbContext.Reclamations.SingleAsync();
        Assert.Equal(StatutReclamation.EnCours, rechargee.Statut);
        Assert.Empty(emailService.Envois);
    }

    [Fact]
    public async Task RepondreAsync_EnregistreLaReponse_PasseLeStatutATraitee_EtEnvoieLaReponseParEmail()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var emailService = new FakeEmailService();
        var reclamationService = new ReclamationService(dbContext, emailService);

        var gestionnaire = new ApplicationUser { UserName = "coach@test.local", Email = "coach@test.local", Prenom = "Coach", Nom = "Test" };
        dbContext.Users.Add(gestionnaire);
        await dbContext.SaveChangesAsync();

        await reclamationService.DeposerAsync(new ReclamationInput { Nom = "Jeanne", Email = "jeanne@test.local", Message = "M" });
        var reclamation = await dbContext.Reclamations.SingleAsync();
        emailService.Envois.Clear();

        var (success, errorMessage) = await reclamationService.RepondreAsync(reclamation.Id, gestionnaire.Id, "Nous avons ajusté votre rythme.");

        Assert.True(success, errorMessage);

        var repondue = await dbContext.Reclamations.SingleAsync();
        Assert.Equal(StatutReclamation.Traitee, repondue.Statut);
        Assert.Equal("Nous avons ajusté votre rythme.", repondue.Reponse);
        Assert.Equal(gestionnaire.Id, repondue.TraiteParId);
        Assert.NotNull(repondue.TraiteLe);

        var email = Assert.Single(emailService.Envois);
        Assert.Equal("jeanne@test.local", email.Destinataire);
        Assert.Contains("Réponse à votre réclamation", email.CorpsHtml);

        var info = await reclamationService.GetByIdAsync(reclamation.Id);
        Assert.NotNull(info);
        Assert.Equal("Coach Test", info!.TraiteParNomComplet);
    }

    [Fact]
    public async Task RepondreAsync_Echoue_SiReponseVide()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var reclamationService = new ReclamationService(dbContext, new FakeEmailService());

        await reclamationService.DeposerAsync(new ReclamationInput { Nom = "A", Email = "a@test.local", Message = "M" });
        var reclamation = await dbContext.Reclamations.SingleAsync();

        var (success, errorMessage) = await reclamationService.RepondreAsync(reclamation.Id, "gestionnaire-id", "   ");

        Assert.False(success);
        Assert.NotNull(errorMessage);

        var toujoursNonTraitee = await dbContext.Reclamations.SingleAsync();
        Assert.Equal(StatutReclamation.Recue, toujoursNonTraitee.Statut);
    }
}
