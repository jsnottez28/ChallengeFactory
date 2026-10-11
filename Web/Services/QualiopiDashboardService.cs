using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

// Agrege uniquement des donnees deja collectees par ailleurs (Cohortes, Preuves,
// Emargements, enquetes de satisfaction/mi-parcours, tests de positionnement amont/aval,
// reclamations) - ne cree, ne modifie ni ne supprime rien. Chaque bloc est independant :
// l'absence de donnees sur l'un (ex. aucune reclamation) ne doit jamais empecher le calcul
// des autres.
public sealed class QualiopiDashboardService(ApplicationDbContext dbContext) : IQualiopiDashboardService
{
    public async Task<QualiopiTableauDeBordInfo> GetTableauDeBordAsync()
    {
        return new QualiopiTableauDeBordInfo
        {
            Cohortes = await GetCohortesAsync(),
            Completion = await GetCompletionAsync(),
            Participation = await GetParticipationAsync(),
            Assiduite = await GetAssiduiteAsync(),
            Satisfaction = await GetSatisfactionAsync(),
            MiParcours = await GetMiParcoursAsync(),
            ProgressionConnaissances = await GetProgressionConnaissancesAsync(),
            Reclamations = await GetReclamationsAsync(),
            GenereLe = DateTime.UtcNow,
        };
    }

    private async Task<QualiopiCohortesInfo> GetCohortesAsync()
    {
        var parStatut = await dbContext.Cohortes
            .GroupBy(c => c.Statut)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        int CompterStatut(StatutCohorte statut) => parStatut.FirstOrDefault(x => x.Key == statut)?.Count ?? 0;

        return new QualiopiCohortesInfo
        {
            // "En preparation" regroupe EnPreparation et Proposee : aucune des deux n'a
            // encore demarre d'etape (cf. StatutCohorte).
            NombreCohortesEnPreparation = CompterStatut(StatutCohorte.EnPreparation) + CompterStatut(StatutCohorte.Proposee),
            NombreCohortesActives = CompterStatut(StatutCohorte.Active),
            NombreCohortesTerminees = CompterStatut(StatutCohorte.Terminee),
            NombreApprenantsTotal = await dbContext.CohorteMembres.Select(m => m.UtilisateurId).Distinct().CountAsync(),
        };
    }

    private async Task<QualiopiCompletionInfo> GetCompletionAsync()
    {
        var nombreCohortesDemarrees = await dbContext.Cohortes
            .CountAsync(c => c.Statut == StatutCohorte.Active || c.Statut == StatutCohorte.Terminee);
        var nombreCohortesTerminees = await dbContext.Cohortes.CountAsync(c => c.Statut == StatutCohorte.Terminee);

        var membresTerminees = await dbContext.CohorteMembres
            .Where(m => m.Cohorte.Statut == StatutCohorte.Terminee)
            .Select(m => new { m.UtilisateurId, m.CohorteId, ChallengeId = m.Cohorte.ChallengeId })
            .ToListAsync();

        var derniereEtapeIdParChallenge = await dbContext.ChallengeEtapes
            .Where(e => membresTerminees.Select(m => m.ChallengeId).Contains(e.ChallengeId))
            .GroupBy(e => e.ChallengeId)
            .Select(g => new { ChallengeId = g.Key, DerniereEtapeId = g.OrderByDescending(e => e.NumeroEtape).Select(e => e.Id).First() })
            .ToDictionaryAsync(x => x.ChallengeId, x => x.DerniereEtapeId);

        var cohorteIdsTerminees = membresTerminees.Select(m => m.CohorteId).Distinct().ToList();
        var validationsDerniereEtape = await dbContext.Preuves
            .Where(p => cohorteIdsTerminees.Contains(p.CohorteId) && p.Statut == StatutPreuve.ValideeDefinitivement)
            .Select(p => new { p.UtilisateurId, p.CohorteId, p.ChallengeEtapeId })
            .ToListAsync();
        var validationsSet = validationsDerniereEtape
            .Select(p => (p.UtilisateurId, p.CohorteId, p.ChallengeEtapeId))
            .ToHashSet();

        var nombreApprenantsAyantValide = membresTerminees.Count(m =>
            derniereEtapeIdParChallenge.TryGetValue(m.ChallengeId, out var derniereEtapeId) &&
            validationsSet.Contains((m.UtilisateurId, m.CohorteId, derniereEtapeId)));

        return new QualiopiCompletionInfo
        {
            NombreCohortesDemarrees = nombreCohortesDemarrees,
            NombreCohortesTerminees = nombreCohortesTerminees,
            TauxCohortesMeneesATermePourcent = nombreCohortesDemarrees > 0
                ? (double)nombreCohortesTerminees / nombreCohortesDemarrees * 100
                : null,
            NombreApprenantsCohortesTerminees = membresTerminees.Count,
            NombreApprenantsAyantValideDerniereEtape = nombreApprenantsAyantValide,
            TauxCompletionApprenantPourcent = membresTerminees.Count > 0
                ? (double)nombreApprenantsAyantValide / membresTerminees.Count * 100
                : null,
        };
    }

    private async Task<QualiopiParticipationInfo> GetParticipationAsync()
    {
        // "Attendues" = pour chaque Cohorte demarree, le nombre d'etapes deja ouvertes
        // (EtapeCourante) x son nombre de membres - approximation volontairement simple
        // (une etape ouverte = un defi individuel attendu), pas un calcul au jour pres.
        var cohortesDemarrees = await dbContext.Cohortes
            .Where(c => c.Statut == StatutCohorte.Active || c.Statut == StatutCohorte.Terminee)
            .Select(c => new { c.Id, c.EtapeCourante, NombreMembres = c.Membres.Count })
            .ToListAsync();

        var nombrePreuvesAttendues = cohortesDemarrees.Sum(c => c.NombreMembres * Math.Max(c.EtapeCourante, 0));
        var cohorteIds = cohortesDemarrees.Select(c => c.Id).ToList();
        var nombrePreuvesDeposees = await dbContext.Preuves.CountAsync(p => cohorteIds.Contains(p.CohorteId));

        return new QualiopiParticipationInfo
        {
            NombrePreuvesAttendues = nombrePreuvesAttendues,
            NombrePreuvesDeposees = nombrePreuvesDeposees,
            TauxParticipationPourcent = nombrePreuvesAttendues > 0
                ? Math.Min(100, (double)nombrePreuvesDeposees / nombrePreuvesAttendues * 100)
                : null,
        };
    }

    private async Task<QualiopiAssiduiteInfo> GetAssiduiteAsync()
    {
        var nombreSeancesProposees = await dbContext.Emargements.CountAsync(e => e.EnvoyeLe != null);
        var nombreSeancesSignees = await dbContext.Emargements.CountAsync(e => e.SigneLe != null);
        var totalHeuresPresence = await dbContext.Emargements
            .Where(e => e.HeuresPresence != null)
            .SumAsync(e => (decimal?)e.HeuresPresence) ?? 0m;

        return new QualiopiAssiduiteInfo
        {
            NombreSeancesProposees = nombreSeancesProposees,
            NombreSeancesSignees = nombreSeancesSignees,
            TauxPresencePourcent = nombreSeancesProposees > 0
                ? (double)nombreSeancesSignees / nombreSeancesProposees * 100
                : null,
            TotalHeuresPresence = totalHeuresPresence,
        };
    }

    private async Task<QualiopiSatisfactionInfo> GetSatisfactionAsync()
    {
        var nombreReponses = await dbContext.SatisfactionReponses.CountAsync();
        if (nombreReponses == 0)
        {
            return new QualiopiSatisfactionInfo { NombreReponses = 0 };
        }

        var promoteurs = await dbContext.SatisfactionReponses.CountAsync(s => s.Score >= 9);
        var detracteurs = await dbContext.SatisfactionReponses.CountAsync(s => s.Score <= 6);

        return new QualiopiSatisfactionInfo
        {
            NombreReponses = nombreReponses,
            ScoreMoyenSur10 = await dbContext.SatisfactionReponses.AverageAsync(s => (double)s.Score),
            NpsPourcent = ((double)promoteurs / nombreReponses - (double)detracteurs / nombreReponses) * 100,
            NoteMoyenneContenus = await dbContext.SatisfactionReponses.AverageAsync(s => (double)s.NoteContenus),
            NoteMoyenneAccompagnement = await dbContext.SatisfactionReponses.AverageAsync(s => (double)s.NoteAccompagnement),
            NoteMoyenneAdequationAttentes = await dbContext.SatisfactionReponses.AverageAsync(s => (double)s.NoteAdequationAttentes),
        };
    }

    private async Task<QualiopiMiParcoursInfo> GetMiParcoursAsync()
    {
        var nombreReponses = await dbContext.QuestionnairesMiParcoursReponses.CountAsync();
        if (nombreReponses == 0)
        {
            return new QualiopiMiParcoursInfo { NombreReponses = 0 };
        }

        return new QualiopiMiParcoursInfo
        {
            NombreReponses = nombreReponses,
            NoteMoyenneAccompagnement = await dbContext.QuestionnairesMiParcoursReponses.AverageAsync(q => (double)q.NoteAccompagnement),
            ProjetPertinentOui = await dbContext.QuestionnairesMiParcoursReponses.CountAsync(q => q.ProjetToujoursPertinent == PertinenceProjet.Oui),
            ProjetPertinentPartiellement = await dbContext.QuestionnairesMiParcoursReponses.CountAsync(q => q.ProjetToujoursPertinent == PertinenceProjet.Partiellement),
            ProjetPertinentNon = await dbContext.QuestionnairesMiParcoursReponses.CountAsync(q => q.ProjetToujoursPertinent == PertinenceProjet.Non),
        };
    }

    private async Task<QualiopiProgressionConnaissancesInfo> GetProgressionConnaissancesAsync()
    {
        var reponses = await dbContext.TestsPositionnementReponses
            .Select(r => new { r.TestPositionnement.CohorteId, r.TestPositionnement.Type, r.NiveauAutoEvalue })
            .ToListAsync();

        var moyenneParCohorteEtType = reponses
            .GroupBy(r => (r.CohorteId, r.Type))
            .Select(g => new { g.Key.CohorteId, g.Key.Type, Moyenne = g.Average(r => (double)r.NiveauAutoEvalue) })
            .ToList();

        var amontParCohorte = moyenneParCohorteEtType
            .Where(x => x.Type == TypeTestPositionnement.Amont)
            .ToDictionary(x => x.CohorteId, x => x.Moyenne);
        var avalParCohorte = moyenneParCohorteEtType
            .Where(x => x.Type == TypeTestPositionnement.Aval)
            .ToDictionary(x => x.CohorteId, x => x.Moyenne);

        // Methode Miroir : on ne compare que les Cohortes ayant effectivement une mesure
        // aux deux bouts du parcours - jamais une Cohorte avec seulement l'un des deux.
        var cohortesAvecLesDeux = amontParCohorte.Keys.Intersect(avalParCohorte.Keys).ToList();
        if (cohortesAvecLesDeux.Count == 0)
        {
            return new QualiopiProgressionConnaissancesInfo { NombreCohortesAvecAmontEtAval = 0 };
        }

        var niveauMoyenAmont = cohortesAvecLesDeux.Average(id => amontParCohorte[id]);
        var niveauMoyenAval = cohortesAvecLesDeux.Average(id => avalParCohorte[id]);

        return new QualiopiProgressionConnaissancesInfo
        {
            NombreCohortesAvecAmontEtAval = cohortesAvecLesDeux.Count,
            NiveauMoyenAmontSur10 = niveauMoyenAmont,
            NiveauMoyenAvalSur10 = niveauMoyenAval,
            ProgressionMoyennePoints = niveauMoyenAval - niveauMoyenAmont,
        };
    }

    private async Task<QualiopiReclamationsInfo> GetReclamationsAsync()
    {
        var nombreTotal = await dbContext.Reclamations.CountAsync();
        var nombreTraitees = await dbContext.Reclamations.CountAsync(r => r.Statut == StatutReclamation.Traitee);

        var delaisTraitement = await dbContext.Reclamations
            .Where(r => r.Statut == StatutReclamation.Traitee && r.TraiteLe != null)
            .Select(r => new { r.DeposeLe, TraiteLe = r.TraiteLe!.Value })
            .ToListAsync();

        return new QualiopiReclamationsInfo
        {
            NombreTotal = nombreTotal,
            NombreRecues = await dbContext.Reclamations.CountAsync(r => r.Statut == StatutReclamation.Recue),
            NombreEnCours = await dbContext.Reclamations.CountAsync(r => r.Statut == StatutReclamation.EnCours),
            NombreTraitees = nombreTraitees,
            TauxTraitementPourcent = nombreTotal > 0 ? (double)nombreTraitees / nombreTotal * 100 : null,
            DelaiMoyenTraitementJours = delaisTraitement.Count > 0
                ? delaisTraitement.Average(r => (r.TraiteLe - r.DeposeLe).TotalDays)
                : null,
        };
    }
}
