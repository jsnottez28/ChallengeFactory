using Application.Common;
using Application.Common.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Extensions;

namespace Web.Services;

public sealed class CarteCompetenceService(ApplicationDbContext dbContext) : ICarteCompetenceService
{
    public async Task<CarteCompetenceResultatRecherche> RechercherAsync(CarteCompetenceFiltre filtre)
    {
        var requete = dbContext.CartesCompetences
            .Include(carte => carte.Badge)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtre.Recherche))
        {
            var recherche = filtre.Recherche.Trim();
            requete = requete.Where(carte =>
                EF.Functions.Like(carte.Code, $"%{recherche}%") ||
                EF.Functions.Like(carte.TitreTheorie, $"%{recherche}%") ||
                (carte.TitreDefi != null && EF.Functions.Like(carte.TitreDefi, $"%{recherche}%")));
        }

        if (filtre.Niveau.HasValue)
        {
            requete = requete.Where(carte => carte.Niveau == filtre.Niveau.Value);
        }

        if (filtre.BadgeId.HasValue)
        {
            requete = requete.Where(carte => carte.BadgeId == filtre.BadgeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtre.Programme))
        {
            requete = requete.Where(carte => carte.Badge != null && carte.Badge.Programme == filtre.Programme);
        }

        var nombreTotal = await requete.CountAsync();

        var page = filtre.Page < 1 ? 1 : filtre.Page;
        var taillePage = filtre.TaillePage < 1 ? 20 : filtre.TaillePage;

        var cartes = await requete
            .OrderBy(carte => carte.Code)
            .Skip((page - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync();

        return new CarteCompetenceResultatRecherche { Cartes = cartes, NombreTotal = nombreTotal };
    }

    public async Task<CarteCompetence?> GetByIdAsync(int id)
    {
        return await dbContext.CartesCompetences
            .Include(carte => carte.Badge)
            .FirstOrDefaultAsync(carte => carte.Id == id);
    }

    public async Task<List<Badge>> GetBadgesAsync()
    {
        return await dbContext.Badges.OrderBy(badge => badge.BadgeNom).ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage, CarteCompetence? Carte)> CreateAsync(CarteCompetenceInput input)
    {
        var erreur = await ValiderAsync(input, carteIdExclue: null);
        if (erreur is not null)
        {
            return (false, erreur, null);
        }

        var carte = new CarteCompetence();
        AppliquerInput(carte, input);

        dbContext.CartesCompetences.Add(carte);
        await dbContext.SaveChangesAsync();

        return (true, null, carte);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(int id, CarteCompetenceInput input)
    {
        var carte = await dbContext.CartesCompetences.FirstOrDefaultAsync(c => c.Id == id);
        if (carte is null)
        {
            return (false, "Carte de compétences introuvable.");
        }

        var erreur = await ValiderAsync(input, carteIdExclue: id);
        if (erreur is not null)
        {
            return (false, erreur);
        }

        AppliquerInput(carte, input);
        carte.ModifieLe = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var carte = await dbContext.CartesCompetences.FirstOrDefaultAsync(c => c.Id == id);
        if (carte is null)
        {
            return (false, "Carte de compétences introuvable.");
        }

        if (await dbContext.CarteAttributions.AnyAsync(a => a.CarteCompetenceId == id && a.EstActif))
        {
            return (false, "Impossible de supprimer cette carte : elle est attribuée à au moins un apprenant. Désattribuez-la d'abord.");
        }

        if (await dbContext.ChallengeEtapeCartes.AnyAsync(ec => ec.CarteCompetenceId == id))
        {
            return (false, "Impossible de supprimer cette carte : elle est utilisée comme Ressource Directrice d'une étape de Challenge. Retirez-la de l'étape d'abord.");
        }

        dbContext.CartesCompetences.Remove(carte);
        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    private async Task<string?> ValiderAsync(CarteCompetenceInput input, int? carteIdExclue)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
        {
            return "Le code de la carte est obligatoire.";
        }

        if (string.IsNullOrWhiteSpace(input.TitreTheorie))
        {
            return "Le titre (face Théorie) est obligatoire.";
        }

        var codeExiste = await dbContext.CartesCompetences
            .AnyAsync(c => c.Code == input.Code && (carteIdExclue == null || c.Id != carteIdExclue));
        if (codeExiste)
        {
            return "Une carte avec ce code existe déjà.";
        }

        if (input.BadgeId.HasValue && !await dbContext.Badges.AnyAsync(b => b.Id == input.BadgeId.Value))
        {
            return "Le badge sélectionné est introuvable.";
        }

        return null;
    }

    private static void AppliquerInput(CarteCompetence carte, CarteCompetenceInput input)
    {
        carte.Code = input.Code.Trim();
        carte.BadgeId = input.BadgeId;
        carte.Niveau = input.Niveau;
        carte.TitreTheorie = input.TitreTheorie.Trim();
        carte.Objectif1 = input.Objectif1;
        carte.Objectif2 = input.Objectif2;
        carte.Objectif3 = input.Objectif3;
        carte.Objectif4 = input.Objectif4;
        carte.Citation = input.Citation;
        carte.AuteurCitation = input.AuteurCitation;
        carte.ImageCarteA = input.ImageCarteA;
        carte.TitreDefi = input.TitreDefi;
        carte.ContextePro = input.ContextePro;
        carte.ContextePerso = input.ContextePerso;
        carte.TonDefi = input.TonDefi;
        carte.Etape1 = input.Etape1;
        carte.Etape2 = input.Etape2;
        carte.Etape3 = input.Etape3;
        carte.Etape4 = input.Etape4;
        carte.Etape5 = input.Etape5;
        carte.Tip1 = input.Tip1;
        carte.Tip2 = input.Tip2;
        carte.Tip3 = input.Tip3;
        carte.Tip4 = input.Tip4;
        carte.Tip5 = input.Tip5;
        carte.CitationHumour = input.CitationHumour;
        carte.LienVideo = input.LienVideo;
    }

    // ---- Import / synchronisation Excel ----

    public async Task<ImportRapport> ImporterAsync(Stream fichierXlsx)
    {
        var rapport = new ImportRapport();

        using var classeur = new XLWorkbook(fichierXlsx);

        var feuilleBadges = classeur.Worksheets.FirstOrDefault(f => f.Name.Equals("badges", StringComparison.OrdinalIgnoreCase));
        var feuilleCartes = classeur.Worksheets.FirstOrDefault(f => f.Name.Equals("cartes", StringComparison.OrdinalIgnoreCase));

        if (feuilleBadges is null)
        {
            rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "badges", Ligne = 0, Raison = "Feuille \"badges\" introuvable dans le fichier." });
        }
        else
        {
            await ImporterBadgesAsync(feuilleBadges, rapport);
        }

        if (feuilleCartes is null)
        {
            rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "cartes", Ligne = 0, Raison = "Feuille \"cartes\" introuvable dans le fichier." });
        }
        else
        {
            await ImporterCartesAsync(feuilleCartes, rapport);
        }

        return rapport;
    }

    private async Task ImporterBadgesAsync(IXLWorksheet feuille, ImportRapport rapport)
    {
        var colonnes = LireEntetes(feuille);
        var badgesExistants = await dbContext.Badges.ToDictionaryAsync(b => b.BadgeCode, StringComparer.OrdinalIgnoreCase);

        foreach (var ligne in LignesDeDonnees(feuille))
        {
            var numeroLigne = ligne.RowNumber();

            try
            {
                var badgeCode = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "badge_code"));
                if (badgeCode is null)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "badges", Ligne = numeroLigne, Champ = "badge_code", Raison = "Champ obligatoire manquant." });
                    continue;
                }

                var badgeNom = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "badge_nom"));
                var badgeImage = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "badge_image"));
                var programme = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "programme"));

                if (badgesExistants.TryGetValue(badgeCode, out var badgeExistant))
                {
                    badgeExistant.BadgeNom = badgeNom ?? badgeExistant.BadgeNom;
                    badgeExistant.BadgeImage = badgeImage;
                    badgeExistant.Programme = programme;
                    rapport.BadgesMisAJour++;
                }
                else
                {
                    var nouveauBadge = new Badge
                    {
                        BadgeCode = badgeCode,
                        BadgeNom = badgeNom ?? badgeCode,
                        BadgeImage = badgeImage,
                        Programme = programme,
                    };
                    dbContext.Badges.Add(nouveauBadge);
                    badgesExistants[badgeCode] = nouveauBadge;
                    rapport.BadgesCrees++;
                }
            }
            catch (Exception ex)
            {
                rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "badges", Ligne = numeroLigne, Raison = ex.Message });
            }
        }

        // Materialise les Id des nouveaux badges avant l'import des cartes (resolution badge_code -> BadgeId).
        await dbContext.SaveChangesAsync();
    }

    private async Task ImporterCartesAsync(IXLWorksheet feuille, ImportRapport rapport)
    {
        var colonnes = LireEntetes(feuille);
        var cartesExistantes = await dbContext.CartesCompetences.ToDictionaryAsync(c => c.Code, StringComparer.OrdinalIgnoreCase);
        var badgesParCode = await dbContext.Badges.ToDictionaryAsync(b => b.BadgeCode, b => b.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var ligne in LignesDeDonnees(feuille))
        {
            var numeroLigne = ligne.RowNumber();

            try
            {
                var code = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "code"));
                if (code is null)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "cartes", Ligne = numeroLigne, Champ = "code", Raison = "Champ obligatoire manquant." });
                    continue;
                }

                var titreTheorie = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "titre_theorie"));
                if (titreTheorie is null)
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "cartes", Ligne = numeroLigne, Champ = "titre_theorie", Raison = "Champ obligatoire manquant." });
                    continue;
                }

                var niveauTexte = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "niveau"));
                if (niveauTexte is null || !TryParserNiveau(niveauTexte, out var niveau))
                {
                    rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "cartes", Ligne = numeroLigne, Champ = "niveau", Raison = $"Valeur \"{niveauTexte}\" non reconnue (attendu : Débutant, Intermédiaire, Moyen, Expert)." });
                    continue;
                }

                int? badgeId = null;
                var badgeCode = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "badge_code"));
                if (badgeCode is not null)
                {
                    if (!badgesParCode.TryGetValue(badgeCode, out var idBadgeResolu))
                    {
                        rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "cartes", Ligne = numeroLigne, Champ = "badge_code", Raison = $"Badge \"{badgeCode}\" introuvable." });
                        continue;
                    }
                    badgeId = idBadgeResolu;
                }

                var carte = cartesExistantes.TryGetValue(code, out var carteExistante) ? carteExistante : null;
                var estNouvelleCarte = carte is null;
                carte ??= new CarteCompetence { Code = code };

                carte.BadgeId = badgeId;
                carte.Niveau = niveau;
                carte.TitreTheorie = titreTheorie;
                carte.Objectif1 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "objectif_1"));
                carte.Objectif2 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "objectif_2"));
                carte.Objectif3 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "objectif_3"));
                carte.Objectif4 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "objectif_4"));
                carte.Citation = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "citation"));
                carte.AuteurCitation = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "auteur_citation"));
                carte.ImageCarteA = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "image_carte_a"));
                carte.TitreDefi = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "titre_defi"));
                carte.ContextePro = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "contexte_pro"));
                carte.ContextePerso = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "contexte_perso"));
                carte.TonDefi = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "ton_defi"));
                carte.Etape1 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "etape_1"));
                carte.Etape2 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "etape_2"));
                carte.Etape3 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "etape_3"));
                carte.Etape4 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "etape_4"));
                carte.Etape5 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "etape_5"));
                carte.Tip1 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "tip_1"));
                carte.Tip2 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "tip_2"));
                carte.Tip3 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "tip_3"));
                carte.Tip4 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "tip_4"));
                carte.Tip5 = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "tip_5"));
                carte.CitationHumour = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "citation_humour"));
                carte.LienVideo = ImportTextNormalizer.Normaliser(ValeurColonne(ligne, colonnes, "lien_video"));

                if (estNouvelleCarte)
                {
                    dbContext.CartesCompetences.Add(carte);
                    cartesExistantes[code] = carte;
                    rapport.CartesCreees++;
                }
                else
                {
                    carte.ModifieLe = DateTime.UtcNow;
                    rapport.CartesMisesAJour++;
                }
            }
            catch (Exception ex)
            {
                rapport.Erreurs.Add(new ImportErreurLigne { Feuille = "cartes", Ligne = numeroLigne, Raison = ex.Message });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static bool TryParserNiveau(string valeur, out NiveauCarte niveau)
    {
        var valeurNormalisee = valeur.Trim();

        foreach (var candidat in Enum.GetValues<NiveauCarte>())
        {
            if (string.Equals(candidat.Libelle(), valeurNormalisee, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(candidat.ToString(), valeurNormalisee, StringComparison.OrdinalIgnoreCase))
            {
                niveau = candidat;
                return true;
            }
        }

        niveau = default;
        return false;
    }

    private static Dictionary<string, int> LireEntetes(IXLWorksheet feuille)
    {
        var entetes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var ligneEntetes = feuille.FirstRowUsed();
        if (ligneEntetes is null)
        {
            return entetes;
        }

        foreach (var cellule in ligneEntetes.CellsUsed())
        {
            var nomColonne = cellule.GetString().Trim();
            if (nomColonne.Length > 0)
            {
                entetes[nomColonne] = cellule.Address.ColumnNumber;
            }
        }

        return entetes;
    }

    private static IEnumerable<IXLRow> LignesDeDonnees(IXLWorksheet feuille)
    {
        var premiereLigne = feuille.FirstRowUsed();
        var derniereLigne = feuille.LastRowUsed();
        if (premiereLigne is null || derniereLigne is null)
        {
            yield break;
        }

        for (var numero = premiereLigne.RowNumber() + 1; numero <= derniereLigne.RowNumber(); numero++)
        {
            var ligne = feuille.Row(numero);
            if (!ligne.CellsUsed().Any())
            {
                continue;
            }

            yield return ligne;
        }
    }

    private static string? ValeurColonne(IXLRow ligne, Dictionary<string, int> colonnes, string nomColonne)
    {
        if (!colonnes.TryGetValue(nomColonne, out var indiceColonne))
        {
            return null;
        }

        var cellule = ligne.Cell(indiceColonne);
        return cellule.IsEmpty() ? null : cellule.GetString();
    }

    // ---- Attribution ----

    public async Task<List<CarteAttributionInfo>> GetAttributionsPourCarteAsync(int carteId)
    {
        return await dbContext.CarteAttributions
            .Include(a => a.CarteCompetence)
            .Include(a => a.Utilisateur)
            .Include(a => a.AttribuePar)
            .Where(a => a.CarteCompetenceId == carteId)
            .OrderByDescending(a => a.AttribueLe)
            .Select(a => VersInfo(a))
            .ToListAsync();
    }

    public async Task<List<CarteAttributionInfo>> GetAttributionsPourUtilisateurAsync(string utilisateurId)
    {
        return await dbContext.CarteAttributions
            .Include(a => a.CarteCompetence)
            .Include(a => a.Utilisateur)
            .Include(a => a.AttribuePar)
            .Where(a => a.UtilisateurId == utilisateurId)
            .OrderByDescending(a => a.AttribueLe)
            .Select(a => VersInfo(a))
            .ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage)> AttribuerAsync(
        List<int> carteIds,
        List<string> utilisateurIds,
        string attribueParId,
        string? contexte)
    {
        if (carteIds.Count == 0 || utilisateurIds.Count == 0)
        {
            return (false, "Sélectionnez au moins une carte et un utilisateur.");
        }

        // Scope strictement aux attributions Libre : une attribution Challenge existante
        // pour la meme paire (carte, utilisateur) ne doit jamais etre reutilisee/ecrasee
        // par une attribution manuelle - ce sont deux origines distinctes (voir
        // OrigineAttribution), chacune avec sa propre ligne.
        var attributionsExistantes = await dbContext.CarteAttributions
            .Where(a => carteIds.Contains(a.CarteCompetenceId)
                && utilisateurIds.Contains(a.UtilisateurId)
                && a.OrigineType == OrigineAttribution.Libre)
            .ToListAsync();

        foreach (var carteId in carteIds)
        {
            foreach (var utilisateurId in utilisateurIds)
            {
                var existante = attributionsExistantes
                    .FirstOrDefault(a => a.CarteCompetenceId == carteId && a.UtilisateurId == utilisateurId);

                if (existante is not null)
                {
                    // Deja attribuee : on reactive si necessaire plutot que de dupliquer.
                    if (!existante.EstActif)
                    {
                        existante.EstActif = true;
                        existante.AttribueParId = attribueParId;
                        existante.AttribueLe = DateTime.UtcNow;
                        existante.Contexte = contexte;
                    }
                    continue;
                }

                dbContext.CarteAttributions.Add(new CarteAttribution
                {
                    CarteCompetenceId = carteId,
                    UtilisateurId = utilisateurId,
                    AttribueParId = attribueParId,
                    AttribueLe = DateTime.UtcNow,
                    Contexte = contexte,
                    EstActif = true,
                    OrigineType = OrigineAttribution.Libre,
                });
            }
        }

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DesattribuerAsync(int attributionId)
    {
        var attribution = await dbContext.CarteAttributions.FirstOrDefaultAsync(a => a.Id == attributionId);
        if (attribution is null)
        {
            return (false, "Attribution introuvable.");
        }

        attribution.EstActif = false;

        await dbContext.SaveChangesAsync();

        return (true, null);
    }

    private static CarteAttributionInfo VersInfo(CarteAttribution a) => new()
    {
        Id = a.Id,
        CarteCompetenceId = a.CarteCompetenceId,
        CarteCode = a.CarteCompetence.Code,
        CarteTitreTheorie = a.CarteCompetence.TitreTheorie,
        UtilisateurId = a.UtilisateurId,
        UtilisateurNomComplet = NomComplet(a.Utilisateur),
        AttribueParId = a.AttribueParId,
        AttribueParNomComplet = NomComplet(a.AttribuePar),
        AttribueLe = a.AttribueLe,
        Contexte = a.Contexte,
        EstActif = a.EstActif,
    };

    private static string NomComplet(ApplicationUser utilisateur)
    {
        var nomComplet = $"{utilisateur.Prenom} {utilisateur.Nom}".Trim();
        return nomComplet.Length > 0 ? nomComplet : (utilisateur.Email ?? utilisateur.UserName ?? utilisateur.Id);
    }
}
