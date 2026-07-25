using ClosedXML.Excel;

namespace Web.Services;

// Lecture generique d'un fichier .xlsx d'import (entetes de colonnes par nom, lignes de
// donnees non vides) - partagee entre tous les imports Excel de la plateforme
// (CarteCompetenceService, ChallengeService) pour eviter de dupliquer cette lecture.
internal static class ExcelImportHelpers
{
    public static Dictionary<string, int> LireEntetes(IXLWorksheet feuille)
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

    public static IEnumerable<IXLRow> LignesDeDonnees(IXLWorksheet feuille)
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

    public static string? ValeurColonne(IXLRow ligne, Dictionary<string, int> colonnes, string nomColonne)
    {
        if (!colonnes.TryGetValue(nomColonne, out var indiceColonne))
        {
            return null;
        }

        var cellule = ligne.Cell(indiceColonne);
        return cellule.IsEmpty() ? null : cellule.GetString();
    }
}
