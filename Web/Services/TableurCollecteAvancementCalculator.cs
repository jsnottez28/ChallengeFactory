using ClosedXML.Excel;

namespace Web.Services;

// Calcule un avancement ESTIMATIF (0-100) a partir d'un tableur de collecte .xlsx depose
// par le client : % des lignes de donnees dont la colonne "Statut" (colonne 11, cf.
// TableurCollecteGenerator.Entetes) vaut "Reçu" ou "Validé". Approximatif par construction
// (un client peut deplacer des colonnes, supprimer des lignes...) - jamais presente comme
// une mesure exacte, seulement comme un repere pour le Gestionnaire.
public static class TableurCollecteAvancementCalculator
{
    private static readonly string[] NomsOngletsExclus = ["Lisez-moi", "Suivi"];

    // Renvoie null si le fichier ne peut pas etre lu comme un classeur Excel valide
    // (corrompu, mauvais format) - a distinguer de 0 (classeur valide mais vide).
    public static int? Calculer(Stream contenu)
    {
        XLWorkbook classeur;
        try
        {
            classeur = new XLWorkbook(contenu);
        }
        catch (Exception)
        {
            return null;
        }

        using (classeur)
        {
            var total = 0;
            var recus = 0;

            foreach (var feuille in classeur.Worksheets.Where(f => !NomsOngletsExclus.Contains(f.Name)))
            {
                foreach (var ligne in feuille.RowsUsed().Where(r => r.RowNumber() >= 4))
                {
                    // Colonne 2 = "Donnée à collecter" : une ligne sans donnee nommee est
                    // une ligne vide (padding du modele genere, ou ligne videe par le
                    // client), jamais comptee.
                    if (string.IsNullOrWhiteSpace(ligne.Cell(2).GetString()))
                    {
                        continue;
                    }

                    total++;
                    if (ligne.Cell(11).GetString() is "Reçu" or "Validé")
                    {
                        recus++;
                    }
                }
            }

            return total == 0 ? 0 : (int)Math.Round(recus * 100.0 / total);
        }
    }
}
