using Domain.Entities;
using Integration.TestSupport;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Services;

namespace Integration.Services;

public class QualiopiDashboardServiceTests
{
    // Seeding direct (pas via ICohorteService/IChallengeService) : le tableau de bord ne
    // fait que lire les tables, pas besoin de rejouer les regles metier de creation/
    // lancement d'une Cohorte pour verifier un calcul d'agregat.
    private static async Task<(Challenge Challenge, ChallengeEtape DerniereEtape)> SemerChallengeAsync(ApplicationDbContext dbContext, int nombreEtapes)
    {
        var challenge = new Challenge { Titre = "Challenge Test", NombreEtapes = nombreEtapes, Mode = ModePlateforme.BtoC };
        dbContext.Challenges.Add(challenge);
        await dbContext.SaveChangesAsync();

        ChallengeEtape derniereEtape = null!;
        for (var i = 1; i <= nombreEtapes; i++)
        {
            var etape = new ChallengeEtape { ChallengeId = challenge.Id, NumeroEtape = i, TitreEtape = $"Étape {i}" };
            dbContext.ChallengeEtapes.Add(etape);
            derniereEtape = etape;
        }
        await dbContext.SaveChangesAsync();

        return (challenge, derniereEtape);
    }

    private static async Task<ApplicationUser> SemerUtilisateurAsync(ApplicationDbContext dbContext, string email)
    {
        var utilisateur = new ApplicationUser { UserName = email, Email = email };
        dbContext.Users.Add(utilisateur);
        await dbContext.SaveChangesAsync();
        return utilisateur;
    }

    [Fact]
    public async Task GetTableauDeBordAsync_SansDonnees_RenvoieDesCompteursAZeroEtDesMoyennesNulles()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var service = new QualiopiDashboardService(dbContext);

        var tableau = await service.GetTableauDeBordAsync();

        Assert.Equal(0, tableau.Cohortes.NombreApprenantsTotal);
        Assert.Null(tableau.Completion.TauxCohortesMeneesATermePourcent);
        Assert.Null(tableau.Participation.TauxParticipationPourcent);
        Assert.Null(tableau.Assiduite.TauxPresencePourcent);
        Assert.Equal(0, tableau.Satisfaction.NombreReponses);
        Assert.Null(tableau.Satisfaction.ScoreMoyenSur10);
        Assert.Equal(0, tableau.ProgressionConnaissances.NombreCohortesAvecAmontEtAval);
        Assert.Equal(0, tableau.Reclamations.NombreTotal);
    }

    [Fact]
    public async Task GetTableauDeBordAsync_CompteLesCohortesParStatutEtLesApprenantsDistincts()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (challenge, _) = await SemerChallengeAsync(dbContext, 1);
        var apprenant = await SemerUtilisateurAsync(dbContext, "apprenant@test.local");

        dbContext.Cohortes.AddRange(
            new Cohorte { ChallengeId = challenge.Id, Nom = "C1", Statut = StatutCohorte.EnPreparation },
            new Cohorte { ChallengeId = challenge.Id, Nom = "C2", Statut = StatutCohorte.Proposee },
            new Cohorte { ChallengeId = challenge.Id, Nom = "C3", Statut = StatutCohorte.Active },
            new Cohorte { ChallengeId = challenge.Id, Nom = "C4", Statut = StatutCohorte.Terminee });
        await dbContext.SaveChangesAsync();

        var cohorteActive = await dbContext.Cohortes.SingleAsync(c => c.Nom == "C3");
        dbContext.CohorteMembres.Add(new CohorteMembre { CohorteId = cohorteActive.Id, UtilisateurId = apprenant.Id, MethodeAjout = MethodeAjoutMembre.Manuel });
        await dbContext.SaveChangesAsync();

        var service = new QualiopiDashboardService(dbContext);
        var tableau = await service.GetTableauDeBordAsync();

        Assert.Equal(2, tableau.Cohortes.NombreCohortesEnPreparation); // EnPreparation + Proposee
        Assert.Equal(1, tableau.Cohortes.NombreCohortesActives);
        Assert.Equal(1, tableau.Cohortes.NombreCohortesTerminees);
        Assert.Equal(1, tableau.Cohortes.NombreApprenantsTotal);
    }

    [Fact]
    public async Task GetTableauDeBordAsync_CalculeLeTauxDeCompletionApprenant_SurLaDerniereEtapeDesCohortesTerminees()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (challenge, derniereEtape) = await SemerChallengeAsync(dbContext, 2);
        var validee = await SemerUtilisateurAsync(dbContext, "validee@test.local");
        var nonValidee = await SemerUtilisateurAsync(dbContext, "nonvalidee@test.local");

        var cohorte = new Cohorte { ChallengeId = challenge.Id, Nom = "C1", Statut = StatutCohorte.Terminee, EtapeCourante = 2 };
        dbContext.Cohortes.Add(cohorte);
        await dbContext.SaveChangesAsync();

        dbContext.CohorteMembres.AddRange(
            new CohorteMembre { CohorteId = cohorte.Id, UtilisateurId = validee.Id, MethodeAjout = MethodeAjoutMembre.Manuel },
            new CohorteMembre { CohorteId = cohorte.Id, UtilisateurId = nonValidee.Id, MethodeAjout = MethodeAjoutMembre.Manuel });
        dbContext.Preuves.Add(new Preuve
        {
            UtilisateurId = validee.Id,
            CohorteId = cohorte.Id,
            ChallengeEtapeId = derniereEtape.Id,
            Statut = StatutPreuve.ValideeDefinitivement,
        });
        await dbContext.SaveChangesAsync();

        var service = new QualiopiDashboardService(dbContext);
        var tableau = await service.GetTableauDeBordAsync();

        Assert.Equal(2, tableau.Completion.NombreApprenantsCohortesTerminees);
        Assert.Equal(1, tableau.Completion.NombreApprenantsAyantValideDerniereEtape);
        Assert.Equal(50, tableau.Completion.TauxCompletionApprenantPourcent);
        Assert.Equal(100, tableau.Completion.TauxCohortesMeneesATermePourcent); // 1 Terminee / 1 demarree
    }

    [Fact]
    public async Task GetTableauDeBordAsync_CalculeLeTauxDeParticipationAuxDefisAttendus()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (challenge, _) = await SemerChallengeAsync(dbContext, 3);
        var membre1 = await SemerUtilisateurAsync(dbContext, "m1@test.local");
        var membre2 = await SemerUtilisateurAsync(dbContext, "m2@test.local");

        // EtapeCourante = 2 => 2 etapes ouvertes x 2 membres = 4 Preuves attendues.
        var cohorte = new Cohorte { ChallengeId = challenge.Id, Nom = "C1", Statut = StatutCohorte.Active, EtapeCourante = 2 };
        dbContext.Cohortes.Add(cohorte);
        await dbContext.SaveChangesAsync();

        dbContext.CohorteMembres.AddRange(
            new CohorteMembre { CohorteId = cohorte.Id, UtilisateurId = membre1.Id, MethodeAjout = MethodeAjoutMembre.Manuel },
            new CohorteMembre { CohorteId = cohorte.Id, UtilisateurId = membre2.Id, MethodeAjout = MethodeAjoutMembre.Manuel });
        var etape1 = await dbContext.ChallengeEtapes.SingleAsync(e => e.ChallengeId == challenge.Id && e.NumeroEtape == 1);
        dbContext.Preuves.Add(new Preuve { UtilisateurId = membre1.Id, CohorteId = cohorte.Id, ChallengeEtapeId = etape1.Id });
        await dbContext.SaveChangesAsync();

        var service = new QualiopiDashboardService(dbContext);
        var tableau = await service.GetTableauDeBordAsync();

        Assert.Equal(4, tableau.Participation.NombrePreuvesAttendues);
        Assert.Equal(1, tableau.Participation.NombrePreuvesDeposees);
        Assert.Equal(25, tableau.Participation.TauxParticipationPourcent);
    }

    [Fact]
    public async Task GetTableauDeBordAsync_CalculeLaSatisfactionEtLeNps()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (challenge, _) = await SemerChallengeAsync(dbContext, 1);
        var cohorte = new Cohorte { ChallengeId = challenge.Id, Nom = "C1", Statut = StatutCohorte.Terminee };
        dbContext.Cohortes.Add(cohorte);
        await dbContext.SaveChangesAsync();

        var promoteur = await SemerUtilisateurAsync(dbContext, "promoteur@test.local");
        var neutre = await SemerUtilisateurAsync(dbContext, "neutre@test.local");
        var detracteur = await SemerUtilisateurAsync(dbContext, "detracteur@test.local");

        dbContext.SatisfactionReponses.AddRange(
            new SatisfactionReponse { CohorteId = cohorte.Id, UtilisateurId = promoteur.Id, Score = 10, NoteContenus = 8, NoteAccompagnement = 9, NoteAdequationAttentes = 7 },
            new SatisfactionReponse { CohorteId = cohorte.Id, UtilisateurId = neutre.Id, Score = 7, NoteContenus = 6, NoteAccompagnement = 6, NoteAdequationAttentes = 6 },
            new SatisfactionReponse { CohorteId = cohorte.Id, UtilisateurId = detracteur.Id, Score = 3, NoteContenus = 4, NoteAccompagnement = 3, NoteAdequationAttentes = 5 });
        await dbContext.SaveChangesAsync();

        var service = new QualiopiDashboardService(dbContext);
        var tableau = await service.GetTableauDeBordAsync();

        Assert.Equal(3, tableau.Satisfaction.NombreReponses);
        Assert.Equal(20.0 / 3, tableau.Satisfaction.ScoreMoyenSur10!.Value, 3); // (10+7+3)/3
        // 1 promoteur (10), 1 detracteur (3) sur 3 reponses : NPS = 1/3 - 1/3 = 0 %.
        Assert.Equal(0, tableau.Satisfaction.NpsPourcent);
    }

    [Fact]
    public async Task GetTableauDeBordAsync_CalculeLaProgressionAmontAval_UniquementSurLesCohortesAyantLesDeux()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var (challenge, etape) = await SemerChallengeAsync(dbContext, 1);
        var apprenant = await SemerUtilisateurAsync(dbContext, "apprenant@test.local");
        var gestionnaire = await SemerUtilisateurAsync(dbContext, "coach@test.local");

        var cohorteAvecLesDeux = new Cohorte { ChallengeId = challenge.Id, Nom = "C1", Statut = StatutCohorte.Terminee };
        var cohorteAmontSeulement = new Cohorte { ChallengeId = challenge.Id, Nom = "C2", Statut = StatutCohorte.Active };
        dbContext.Cohortes.AddRange(cohorteAvecLesDeux, cohorteAmontSeulement);
        await dbContext.SaveChangesAsync();

        var amont1 = new TestPositionnement { CohorteId = cohorteAvecLesDeux.Id, Type = TypeTestPositionnement.Amont, EnvoyeParId = gestionnaire.Id };
        var aval1 = new TestPositionnement { CohorteId = cohorteAvecLesDeux.Id, Type = TypeTestPositionnement.Aval, EnvoyeParId = gestionnaire.Id };
        var amont2 = new TestPositionnement { CohorteId = cohorteAmontSeulement.Id, Type = TypeTestPositionnement.Amont, EnvoyeParId = gestionnaire.Id };
        dbContext.TestsPositionnement.AddRange(amont1, aval1, amont2);
        await dbContext.SaveChangesAsync();

        dbContext.TestsPositionnementReponses.AddRange(
            new TestPositionnementReponse { TestPositionnementId = amont1.Id, UtilisateurId = apprenant.Id, ChallengeEtapeId = etape.Id, NiveauAutoEvalue = 3 },
            new TestPositionnementReponse { TestPositionnementId = aval1.Id, UtilisateurId = apprenant.Id, ChallengeEtapeId = etape.Id, NiveauAutoEvalue = 8 },
            new TestPositionnementReponse { TestPositionnementId = amont2.Id, UtilisateurId = apprenant.Id, ChallengeEtapeId = etape.Id, NiveauAutoEvalue = 1 });
        await dbContext.SaveChangesAsync();

        var service = new QualiopiDashboardService(dbContext);
        var tableau = await service.GetTableauDeBordAsync();

        // Seule cohorteAvecLesDeux compte (cohorteAmontSeulement n'a pas d'Aval).
        Assert.Equal(1, tableau.ProgressionConnaissances.NombreCohortesAvecAmontEtAval);
        Assert.Equal(3, tableau.ProgressionConnaissances.NiveauMoyenAmontSur10);
        Assert.Equal(8, tableau.ProgressionConnaissances.NiveauMoyenAvalSur10);
        Assert.Equal(5, tableau.ProgressionConnaissances.ProgressionMoyennePoints);
    }

    [Fact]
    public async Task GetTableauDeBordAsync_CalculeLeTauxDeTraitementEtLeDelaiMoyenDesReclamations()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var depot = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dbContext.Reclamations.AddRange(
            new Reclamation { Nom = "A", Email = "a@test.local", Message = "msg", Statut = StatutReclamation.Recue, DeposeLe = depot },
            new Reclamation { Nom = "B", Email = "b@test.local", Message = "msg", Statut = StatutReclamation.EnCours, DeposeLe = depot },
            new Reclamation { Nom = "C", Email = "c@test.local", Message = "msg", Statut = StatutReclamation.Traitee, DeposeLe = depot, TraiteLe = depot.AddDays(4) });
        await dbContext.SaveChangesAsync();

        var service = new QualiopiDashboardService(dbContext);
        var tableau = await service.GetTableauDeBordAsync();

        Assert.Equal(3, tableau.Reclamations.NombreTotal);
        Assert.Equal(1, tableau.Reclamations.NombreRecues);
        Assert.Equal(1, tableau.Reclamations.NombreEnCours);
        Assert.Equal(1, tableau.Reclamations.NombreTraitees);
        Assert.Equal(100.0 / 3, tableau.Reclamations.TauxTraitementPourcent!.Value, 3);
        Assert.Equal(4, tableau.Reclamations.DelaiMoyenTraitementJours);
    }
}
