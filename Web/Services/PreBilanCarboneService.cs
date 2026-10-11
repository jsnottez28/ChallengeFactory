using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Services;

public sealed class PreBilanCarboneService(ApplicationDbContext dbContext, IEmailService emailService) : IPreBilanCarboneService
{
    public async Task<List<FacteurEmissionInfo>> GetFacteursDisponiblesAsync()
    {
        var facteurs = await dbContext.FacteursEmission
            .Where(f => f.Actif)
            .OrderBy(f => f.Poste)
            .ThenBy(f => f.Ordre)
            .ToListAsync();

        return facteurs.Select(VersFacteurInfo).ToList();
    }

    public async Task<(bool Success, string? ErrorMessage, PreBilanResultatInfo? Resultat)> DeposerAsync(PreBilanCarboneInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Email))
        {
            return (false, "L'adresse email est obligatoire.", null);
        }

        var facteurs = await dbContext.FacteursEmission.Where(f => f.Actif).ToListAsync();
        var facteursParCode = facteurs.ToDictionary(f => f.Code);

        var lignes = new List<PreBilanLigne>();
        foreach (var (code, valeur) in input.ValeursParCodeFacteur)
        {
            if (valeur <= 0 || !facteursParCode.TryGetValue(code, out var facteur))
            {
                continue;
            }

            lignes.Add(new PreBilanLigne
            {
                FacteurEmissionId = facteur.Id,
                FacteurEmission = facteur,
                ValeurSaisie = valeur,
                EmissionsKgCO2e = valeur * facteur.ValeurKgCO2eParUnite,
            });
        }

        if (lignes.Count == 0)
        {
            return (false, "Merci de renseigner au moins une donnée avant de calculer votre estimation.", null);
        }

        var preBilan = new PreBilanCarbone
        {
            Email = input.Email.Trim(),
            Nom = ValeurOuNull(input.Nom),
            Prenom = ValeurOuNull(input.Prenom),
            Societe = ValeurOuNull(input.Societe),
            Telephone = ValeurOuNull(input.Telephone),
            SecteurActivite = ValeurOuNull(input.SecteurActivite),
            EffectifEtp = input.EffectifEtp,
            ChiffreAffairesKEuros = input.ChiffreAffairesKEuros,
            TotalEmissionsKgCO2e = lignes.Sum(l => l.EmissionsKgCO2e),
            Lignes = lignes,
        };

        dbContext.PreBilansCarbone.Add(preBilan);
        await dbContext.SaveChangesAsync();

        var resultat = VersResultatInfo(preBilan, lignes);

        var (sujet, corps) = ChallengeEmailTemplates.ResultatPreBilanCarbone(resultat);
        await emailService.EnvoyerAsync(preBilan.Email, sujet, corps);

        return (true, null, resultat);
    }

    public async Task<List<PreBilanCarboneInfo>> GetTousAsync()
    {
        var preBilans = await dbContext.PreBilansCarbone
            .OrderByDescending(p => p.DeposeLe)
            .ToListAsync();

        return preBilans.Select(VersCrmInfo).ToList();
    }

    public async Task<PreBilanResultatInfo?> GetResultatAsync(int id)
    {
        var preBilan = await dbContext.PreBilansCarbone
            .Include(p => p.Lignes).ThenInclude(l => l.FacteurEmission)
            .FirstOrDefaultAsync(p => p.Id == id);

        return preBilan is null ? null : VersResultatInfo(preBilan, preBilan.Lignes);
    }

    public async Task<(bool Success, string? ErrorMessage)> ChangerStatutCrmAsync(int id, StatutPreBilanCrm statut)
    {
        var preBilan = await dbContext.PreBilansCarbone.FirstOrDefaultAsync(p => p.Id == id);
        if (preBilan is null)
        {
            return (false, "Pré-diagnostic introuvable.");
        }

        preBilan.StatutCrm = statut;
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    private static string? ValeurOuNull(string? valeur) => string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();

    private static FacteurEmissionInfo VersFacteurInfo(FacteurEmission f) => new()
    {
        Id = f.Id,
        Code = f.Code,
        Poste = f.Poste,
        TypeDonnee = f.TypeDonnee,
        Nom = f.Nom,
        Unite = f.Unite,
    };

    private static PreBilanResultatInfo VersResultatInfo(PreBilanCarbone preBilan, List<PreBilanLigne> lignes) => new()
    {
        Id = preBilan.Id,
        TotalEmissionsKgCO2e = preBilan.TotalEmissionsKgCO2e,
        DeposeLe = preBilan.DeposeLe,
        ParPoste = lignes
            .GroupBy(l => l.FacteurEmission.Poste)
            .Select(g => new PreBilanPosteResultatInfo { Poste = g.Key, EmissionsTotalesKgCO2e = g.Sum(l => l.EmissionsKgCO2e) })
            .OrderBy(p => p.Poste)
            .ToList(),
        Lignes = lignes
            .Select(l => new PreBilanLigneResultatInfo
            {
                Code = l.FacteurEmission.Code,
                Nom = l.FacteurEmission.Nom,
                Poste = l.FacteurEmission.Poste,
                ValeurSaisie = l.ValeurSaisie,
                EmissionsKgCO2e = l.EmissionsKgCO2e,
            })
            .OrderByDescending(l => l.EmissionsKgCO2e)
            .ToList(),
    };

    private static PreBilanCarboneInfo VersCrmInfo(PreBilanCarbone p) => new()
    {
        Id = p.Id,
        Email = p.Email,
        Nom = p.Nom,
        Prenom = p.Prenom,
        Societe = p.Societe,
        Telephone = p.Telephone,
        SecteurActivite = p.SecteurActivite,
        EffectifEtp = p.EffectifEtp,
        ChiffreAffairesKEuros = p.ChiffreAffairesKEuros,
        TotalEmissionsKgCO2e = p.TotalEmissionsKgCO2e,
        DeposeLe = p.DeposeLe,
        StatutCrm = p.StatutCrm,
    };
}
