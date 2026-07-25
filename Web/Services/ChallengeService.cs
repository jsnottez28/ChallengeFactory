using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Web.Services;

public sealed class ChallengeService(ApplicationDbContext dbContext) : IChallengeService
{
    public async Task<List<Challenge>> GetAllAsync()
    {
        return await dbContext.Challenges
            .OrderByDescending(c => c.CreeLe)
            .ToListAsync();
    }

    public async Task<Challenge?> GetByIdAsync(int id)
    {
        return await dbContext.Challenges
            .Include(c => c.Etapes.OrderBy(e => e.NumeroEtape))
                .ThenInclude(e => e.Cartes)
                    .ThenInclude(ec => ec.CarteCompetence)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ChallengeEtape?> GetEtapeByIdAsync(int etapeId)
    {
        return await dbContext.ChallengeEtapes
            .Include(e => e.Cartes)
            .FirstOrDefaultAsync(e => e.Id == etapeId);
    }

    public async Task<(bool Success, string? ErrorMessage, Challenge? Challenge)> CreateAsync(ChallengeInput input)
    {
        var erreur = Valider(input);
        if (erreur is not null)
        {
            return (false, erreur, null);
        }

        var challenge = new Challenge
        {
            Titre = input.Titre.Trim(),
            Slogan = input.Slogan,
            NombreEtapes = input.NombreEtapes,
            Mode = input.Mode,
        };

        dbContext.Challenges.Add(challenge);
        await dbContext.SaveChangesAsync();

        return (true, null, challenge);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, ChallengeInput input)
    {
        var challenge = await dbContext.Challenges
            .Include(c => c.Etapes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge is null)
        {
            return (false, "Challenge introuvable.");
        }

        var erreur = Valider(input);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        if (input.NombreEtapes < challenge.Etapes.Count)
        {
            return (false, $"Le nombre d'étapes ne peut pas être réduit en dessous du nombre d'étapes déjà créées ({challenge.Etapes.Count}).");
        }

        challenge.Titre = input.Titre.Trim();
        challenge.Slogan = input.Slogan;
        challenge.NombreEtapes = input.NombreEtapes;
        challenge.Mode = input.Mode;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> PublierAsync(int id)
    {
        var challenge = await dbContext.Challenges
            .Include(c => c.Etapes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge is null)
        {
            return (false, "Challenge introuvable.");
        }

        if (challenge.Statut == StatutChallenge.Publie)
        {
            return (false, "Ce Challenge est déjà publié.");
        }

        if (challenge.Etapes.Count == 0)
        {
            return (false, "Impossible de publier un Challenge sans étape.");
        }

        challenge.Statut = StatutChallenge.Publie;
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage, ChallengeEtape? Etape)> CreerEtapeAsync(int challengeId, ChallengeEtapeInput input)
    {
        var challenge = await dbContext.Challenges
            .Include(c => c.Etapes)
            .FirstOrDefaultAsync(c => c.Id == challengeId);

        if (challenge is null)
        {
            return (false, "Challenge introuvable.", null);
        }

        var erreurVerrouillage = await VerifierArchitectureModifiableAsync(challengeId);
        if (erreurVerrouillage is not null)
        {
            return (false, erreurVerrouillage, null);
        }

        if (string.IsNullOrWhiteSpace(input.TitreEtape))
        {
            return (false, "Le titre de l'étape est obligatoire.", null);
        }

        if (challenge.Etapes.Count >= challenge.NombreEtapes)
        {
            return (false, $"Ce Challenge est limité à {challenge.NombreEtapes} étape(s). Augmentez le nombre d'étapes avant d'en ajouter une nouvelle.", null);
        }

        var prochainNumero = challenge.Etapes.Count == 0 ? 1 : challenge.Etapes.Max(e => e.NumeroEtape) + 1;

        var etape = new ChallengeEtape
        {
            ChallengeId = challengeId,
            NumeroEtape = prochainNumero,
            TitreEtape = input.TitreEtape.Trim(),
            ObjectifPedagogique = input.ObjectifPedagogique,
            CompetenceCible = input.CompetenceCible,
            DefiIndividuel = input.DefiIndividuel,
        };

        dbContext.ChallengeEtapes.Add(etape);
        await dbContext.SaveChangesAsync();

        return (true, null, etape);
    }

    public async Task<(bool Success, string? ErrorMessage)> ModifierEtapeAsync(int etapeId, ChallengeEtapeInput input)
    {
        var etape = await dbContext.ChallengeEtapes.FirstOrDefaultAsync(e => e.Id == etapeId);
        if (etape is null)
        {
            return (false, "Étape introuvable.");
        }

        var erreurVerrouillage = await VerifierArchitectureModifiableAsync(etape.ChallengeId);
        if (erreurVerrouillage is not null)
        {
            return (false, erreurVerrouillage);
        }

        if (string.IsNullOrWhiteSpace(input.TitreEtape))
        {
            return (false, "Le titre de l'étape est obligatoire.");
        }

        etape.TitreEtape = input.TitreEtape.Trim();
        etape.ObjectifPedagogique = input.ObjectifPedagogique;
        etape.CompetenceCible = input.CompetenceCible;
        etape.DefiIndividuel = input.DefiIndividuel;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> SupprimerEtapeAsync(int etapeId)
    {
        var etape = await dbContext.ChallengeEtapes.FirstOrDefaultAsync(e => e.Id == etapeId);
        if (etape is null)
        {
            return (false, "Étape introuvable.");
        }

        var erreurVerrouillage = await VerifierArchitectureModifiableAsync(etape.ChallengeId);
        if (erreurVerrouillage is not null)
        {
            return (false, erreurVerrouillage);
        }

        dbContext.ChallengeEtapes.Remove(etape);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DefinirCartesEtapeAsync(int etapeId, List<int> carteCompetenceIds)
    {
        var etape = await dbContext.ChallengeEtapes
            .Include(e => e.Cartes)
            .FirstOrDefaultAsync(e => e.Id == etapeId);

        if (etape is null)
        {
            return (false, "Étape introuvable.");
        }

        var erreurVerrouillage = await VerifierArchitectureModifiableAsync(etape.ChallengeId);
        if (erreurVerrouillage is not null)
        {
            return (false, erreurVerrouillage);
        }

        var idsSouhaites = carteCompetenceIds.Distinct().ToList();
        var idsActuels = etape.Cartes.Select(c => c.CarteCompetenceId).ToList();

        var aRetirer = etape.Cartes.Where(c => !idsSouhaites.Contains(c.CarteCompetenceId)).ToList();
        foreach (var carte in aRetirer)
        {
            dbContext.ChallengeEtapeCartes.Remove(carte);
        }

        var aAjouter = idsSouhaites.Except(idsActuels);
        foreach (var carteId in aAjouter)
        {
            dbContext.ChallengeEtapeCartes.Add(new ChallengeEtapeCarte
            {
                ChallengeEtapeId = etapeId,
                CarteCompetenceId = carteId,
            });
        }

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    // Bloque toute modification de l'architecture (etapes, cartes rattachees) des qu'une
    // Cohorte issue de ce Challenge a ete lancee (Active ou Terminee) : une Cohorte
    // En preparation n'a pas encore consomme l'architecture, la modifier reste sans risque.
    private async Task<string?> VerifierArchitectureModifiableAsync(int challengeId)
    {
        var aUneCohorteLancee = await dbContext.Cohortes.AnyAsync(co =>
            co.ChallengeId == challengeId &&
            (co.Statut == StatutCohorte.Active || co.Statut == StatutCohorte.Terminee));

        return aUneCohorteLancee
            ? "Impossible de modifier l'architecture de ce Challenge : au moins une Cohorte a déjà été lancée à partir de celui-ci."
            : null;
    }

    private static string? Valider(ChallengeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Titre))
        {
            return "Le titre du Challenge est obligatoire.";
        }

        if (input.NombreEtapes < 1)
        {
            return "Le nombre d'étapes doit être d'au moins 1.";
        }

        return null;
    }
}
