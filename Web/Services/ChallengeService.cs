using Application.Common;
using Application.Common.Interfaces;
using ClosedXML.Excel;
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
        var erreur = await ValiderAsync(input, challengeIdExclu: null);
        if (erreur is not null)
        {
            return (false, erreur, null);
        }

        var challenge = new Challenge
        {
            Code = NormaliserCode(input.Code),
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

        var erreur = await ValiderAsync(input, challengeIdExclu: id);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        if (input.NombreEtapes < challenge.Etapes.Count)
        {
            return (false, $"Le nombre d'étapes ne peut pas être réduit en dessous du nombre d'étapes déjà créées ({challenge.Etapes.Count}).");
        }

        challenge.Code = NormaliserCode(input.Code);
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

    public async Task<(bool Success, string? ErrorMessage)> SupprimerAsync(int id)
    {
        var challenge = await dbContext.Challenges.FirstOrDefaultAsync(c => c.Id == id);
        if (challenge is null)
        {
            return (false, "Challenge introuvable.");
        }

        if (challenge.Statut != StatutChallenge.Brouillon)
        {
            return (false, "Impossible de supprimer un Challenge déjà publié. Il ne peut plus être un Brouillon une fois utilisé comme modèle de Cohorte.");
        }

        // Un Challenge Brouillon ne peut par construction avoir aucune Cohorte rattachee
        // (CreateAsync de CohorteService exige un Challenge Publie) - controle de defense
        // en profondeur au cas ou cette regle evoluerait.
        var aUneCohorte = await dbContext.Cohortes.AnyAsync(co => co.ChallengeId == id);
        if (aUneCohorte)
        {
            return (false, "Impossible de supprimer ce Challenge : au moins une Cohorte y est rattachée.");
        }

        dbContext.Challenges.Remove(challenge);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    // ---- Import / synchronisation Excel ----

    // Fichier .xlsx a 2 feuilles :
    // - "challenge" : challenge_code, titre, slogan, nombre_etapes, mode, statut
    //   (upsert par challenge_code ; statut="Publie" ne publie qu'en fin d'import,
    //   apres import des etapes, et seulement si le Challenge en a au moins une).
    // - "etapes" : challenge_code, numero_etape, titre_etape, objectif_pedagogique,
    //   competence_cible, defi_individuel, codes_cartes (codes CarteCompetence separes
    //   par virgule/point-virgule ; remplace integralement les cartes de l'etape, comme
    //   DefinirCartesEtapeAsync). Upsert par (challenge_code, numero_etape).
    public async Task<ImportChallengeRapport> ImporterAsync(Stream fichierXlsx)
    {
        var rapport = new ImportChallengeRapport();

        using var classeur = new XLWorkbook(fichierXlsx);

        var feuilleChallenge = classeur.Worksheets.FirstOrDefault(f => f.Name.Equals("challenge", StringComparison.OrdinalIgnoreCase));
        var feuilleEtapes = classeur.Worksheets.FirstOrDefault(f => f.Name.Equals("etapes", StringComparison.OrdinalIgnoreCase));

        if (feuilleChallenge is null)
        {
            rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = 0, Raison = "Feuille \"challenge\" introuvable dans le fichier." });
            return rapport;
        }

        var codesAPublier = await ImporterChallengesAsync(feuilleChallenge, rapport);

        if (feuilleEtapes is null)
        {
            rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = 0, Raison = "Feuille \"etapes\" introuvable dans le fichier." });
        }
        else
        {
            await ImporterEtapesAsync(feuilleEtapes, rapport);
        }

        await PublierChallengesDemandesAsync(codesAPublier, rapport);

        return rapport;
    }

    private async Task<List<string>> ImporterChallengesAsync(IXLWorksheet feuille, ImportChallengeRapport rapport)
    {
        var colonnes = ExcelImportHelpers.LireEntetes(feuille);
        var challengesExistants = await dbContext.Challenges
            .Where(c => c.Code != null)
            .ToDictionaryAsync(c => c.Code!, StringComparer.OrdinalIgnoreCase);

        var codesAPublier = new List<string>();

        foreach (var ligne in ExcelImportHelpers.LignesDeDonnees(feuille))
        {
            var numeroLigne = ligne.RowNumber();

            try
            {
                var code = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "challenge_code"));
                if (code is null)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = numeroLigne, Champ = "challenge_code", Raison = "Champ obligatoire manquant." });
                    continue;
                }

                var titre = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "titre"));
                if (titre is null)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = numeroLigne, Champ = "titre", Raison = "Champ obligatoire manquant." });
                    continue;
                }

                var modeTexte = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "mode"));
                if (modeTexte is null || !TryParserMode(modeTexte, out var mode))
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = numeroLigne, Champ = "mode", Raison = $"Valeur \"{modeTexte}\" non reconnue (attendu : BtoB, BtoC)." });
                    continue;
                }

                var nombreEtapesTexte = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "nombre_etapes"));
                var nombreEtapes = 8;
                if (nombreEtapesTexte is not null && (!int.TryParse(nombreEtapesTexte, out nombreEtapes) || nombreEtapes < 1))
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = numeroLigne, Champ = "nombre_etapes", Raison = $"Valeur \"{nombreEtapesTexte}\" invalide (entier >= 1 attendu)." });
                    continue;
                }

                var estNouveau = !challengesExistants.TryGetValue(code, out var challengeExistant);
                Challenge challenge;

                if (!estNouveau)
                {
                    var nombreEtapesActuelles = await dbContext.ChallengeEtapes.CountAsync(e => e.ChallengeId == challengeExistant!.Id);
                    if (nombreEtapes < nombreEtapesActuelles)
                    {
                        rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = numeroLigne, Champ = "nombre_etapes", Raison = $"Ne peut pas être réduit en dessous du nombre d'étapes déjà créées ({nombreEtapesActuelles})." });
                        continue;
                    }

                    challenge = challengeExistant!;
                }
                else
                {
                    challenge = new Challenge { Code = code };
                }

                challenge.Titre = titre;
                challenge.Slogan = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "slogan"));
                challenge.NombreEtapes = nombreEtapes;
                challenge.Mode = mode;

                if (estNouveau)
                {
                    dbContext.Challenges.Add(challenge);
                    challengesExistants[code] = challenge;
                    rapport.ChallengesCrees++;
                }
                else
                {
                    rapport.ChallengesMisAJour++;
                }

                var statutTexte = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "statut"));
                if (statutTexte is not null && EstStatutPublie(statutTexte))
                {
                    codesAPublier.Add(code);
                }
            }
            catch (Exception ex)
            {
                rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = numeroLigne, Raison = ex.Message });
            }
        }

        // Materialise les Id des nouveaux Challenges avant l'import des etapes (resolution
        // challenge_code -> ChallengeId).
        await dbContext.SaveChangesAsync();

        return codesAPublier;
    }

    private async Task ImporterEtapesAsync(IXLWorksheet feuille, ImportChallengeRapport rapport)
    {
        var colonnes = ExcelImportHelpers.LireEntetes(feuille);
        var challengesParCode = await dbContext.Challenges
            .Where(c => c.Code != null)
            .ToDictionaryAsync(c => c.Code!, StringComparer.OrdinalIgnoreCase);
        var cartesParCode = await dbContext.CartesCompetences
            .ToDictionaryAsync(c => c.Code, c => c.Id, StringComparer.OrdinalIgnoreCase);

        // Cache la verification "architecture modifiable" par Challenge : evite de la
        // reinterroger a chaque ligne d'etape du meme Challenge.
        var challengesDeverrouilles = new HashSet<int>();
        var challengesVerrouilles = new Dictionary<int, string>();

        foreach (var ligne in ExcelImportHelpers.LignesDeDonnees(feuille))
        {
            var numeroLigne = ligne.RowNumber();

            try
            {
                var challengeCode = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "challenge_code"));
                if (challengeCode is null)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "challenge_code", Raison = "Champ obligatoire manquant." });
                    continue;
                }

                if (!challengesParCode.TryGetValue(challengeCode, out var challenge))
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "challenge_code", Raison = $"Challenge \"{challengeCode}\" introuvable (absent de la feuille \"challenge\" et de la base)." });
                    continue;
                }

                if (!challengesDeverrouilles.Contains(challenge.Id))
                {
                    if (challengesVerrouilles.TryGetValue(challenge.Id, out var raisonVerrouillage))
                    {
                        rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "challenge_code", Raison = raisonVerrouillage });
                        continue;
                    }

                    var erreurVerrouillage = await VerifierArchitectureModifiableAsync(challenge.Id);
                    if (erreurVerrouillage is not null)
                    {
                        challengesVerrouilles[challenge.Id] = erreurVerrouillage;
                        rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "challenge_code", Raison = erreurVerrouillage });
                        continue;
                    }

                    challengesDeverrouilles.Add(challenge.Id);
                }

                var numeroTexte = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "numero_etape"));
                if (numeroTexte is null || !int.TryParse(numeroTexte, out var numeroEtape) || numeroEtape < 1)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "numero_etape", Raison = $"Valeur \"{numeroTexte}\" invalide (entier >= 1 attendu)." });
                    continue;
                }

                if (numeroEtape > challenge.NombreEtapes)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "numero_etape", Raison = $"Dépasse le nombre d'étapes déclaré pour ce Challenge ({challenge.NombreEtapes})." });
                    continue;
                }

                var titreEtape = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "titre_etape"));
                if (titreEtape is null)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "titre_etape", Raison = "Champ obligatoire manquant." });
                    continue;
                }

                var etape = await dbContext.ChallengeEtapes
                    .Include(e => e.Cartes)
                    .FirstOrDefaultAsync(e => e.ChallengeId == challenge.Id && e.NumeroEtape == numeroEtape);
                var estNouvelleEtape = etape is null;
                etape ??= new ChallengeEtape { ChallengeId = challenge.Id, NumeroEtape = numeroEtape };

                etape.TitreEtape = titreEtape;
                etape.ObjectifPedagogique = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "objectif_pedagogique"));
                etape.CompetenceCible = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "competence_cible"));
                etape.DefiIndividuel = ImportTextNormalizer.Normaliser(ExcelImportHelpers.ValeurColonne(ligne, colonnes, "defi_individuel"));

                if (estNouvelleEtape)
                {
                    dbContext.ChallengeEtapes.Add(etape);
                    rapport.EtapesCreees++;
                    // Materialise l'Id de l'etape avant de rattacher ses cartes ci-dessous.
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    rapport.EtapesMisesAJour++;
                }

                var codesCartesTexte = ExcelImportHelpers.ValeurColonne(ligne, colonnes, "codes_cartes");
                var codesCartes = string.IsNullOrWhiteSpace(codesCartesTexte)
                    ? []
                    : codesCartesTexte.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var idsCartesResolues = new List<int>();
                foreach (var codeCarte in codesCartes)
                {
                    if (cartesParCode.TryGetValue(codeCarte, out var carteId))
                    {
                        idsCartesResolues.Add(carteId);
                    }
                    else
                    {
                        rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Champ = "codes_cartes", Raison = $"Carte \"{codeCarte}\" introuvable." });
                    }
                }

                var idsSouhaites = idsCartesResolues.Distinct().ToList();
                var idsActuels = etape.Cartes.Select(c => c.CarteCompetenceId).ToList();

                var aRetirer = etape.Cartes.Where(c => !idsSouhaites.Contains(c.CarteCompetenceId)).ToList();
                foreach (var carte in aRetirer)
                {
                    dbContext.ChallengeEtapeCartes.Remove(carte);
                }

                var aAjouter = idsSouhaites.Except(idsActuels);
                foreach (var carteId in aAjouter)
                {
                    dbContext.ChallengeEtapeCartes.Add(new ChallengeEtapeCarte { ChallengeEtapeId = etape.Id, CarteCompetenceId = carteId });
                    rapport.CartesRattachees++;
                }
            }
            catch (Exception ex)
            {
                rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "etapes", Ligne = numeroLigne, Raison = ex.Message });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task PublierChallengesDemandesAsync(List<string> codes, ImportChallengeRapport rapport)
    {
        foreach (var code in codes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var challenge = await dbContext.Challenges
                .Include(c => c.Etapes)
                .FirstOrDefaultAsync(c => c.Code == code);

            if (challenge is null || challenge.Statut == StatutChallenge.Publie)
            {
                continue;
            }

            if (challenge.Etapes.Count == 0)
            {
                rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "challenge", Ligne = 0, Champ = "statut", Raison = $"Challenge \"{code}\" : impossible de publier, aucune étape importée. Reste en Brouillon." });
                continue;
            }

            challenge.Statut = StatutChallenge.Publie;
            rapport.ChallengesPublies++;
        }

        await dbContext.SaveChangesAsync();
    }

    private static bool TryParserMode(string valeur, out ModePlateforme mode)
    {
        if (string.Equals(valeur, "BtoB", StringComparison.OrdinalIgnoreCase))
        {
            mode = ModePlateforme.BtoB;
            return true;
        }

        if (string.Equals(valeur, "BtoC", StringComparison.OrdinalIgnoreCase))
        {
            mode = ModePlateforme.BtoC;
            return true;
        }

        mode = default;
        return false;
    }

    private static bool EstStatutPublie(string valeur) =>
        string.Equals(valeur, "Publie", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(valeur, "Publié", StringComparison.OrdinalIgnoreCase);

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

    private async Task<string?> ValiderAsync(ChallengeInput input, int? challengeIdExclu)
    {
        if (string.IsNullOrWhiteSpace(input.Titre))
        {
            return "Le titre du Challenge est obligatoire.";
        }

        if (input.NombreEtapes < 1)
        {
            return "Le nombre d'étapes doit être d'au moins 1.";
        }

        var code = NormaliserCode(input.Code);
        if (code is not null)
        {
            var codeExiste = await dbContext.Challenges
                .AnyAsync(c => c.Code == code && (challengeIdExclu == null || c.Id != challengeIdExclu));
            if (codeExiste)
            {
                return "Un Challenge avec ce code existe déjà.";
            }
        }

        return null;
    }

    private static string? NormaliserCode(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : code.Trim();
}
