using Application.Common.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;

namespace Web.Services;

// Extrait du tableur de collecte .xlsx depose par le client les valeurs reellement
// saisies, ligne par ligne : une ligne dont la cle (Poste+Sous-poste+Donnee) correspond a
// un element connu du catalogue alimente ValeurSaisieFichier ; une ligne non reconnue
// (ex. ligne complementaire ajoutee par le client pour un second site) est capturee a part
// (cf. LigneSupplementaireCollecte), jamais perdue. Memes limites que
// TableurCollecteAvancementCalculator : approximatif si le client renomme un onglet ou
// modifie les colonnes Sous-poste/Donnee, qui portent la cle.
public static class TableurCollecteValeursExtracteur
{
    private static readonly string[] NomsOngletsExclus = ["Lisez-moi", "Suivi"];

    public sealed record Resultat(Dictionary<string, ValeurSaisieFichier> Valeurs, List<LigneSupplementaireCollecte> LignesSupplementaires);

    // Renvoie null si le fichier ne peut pas etre lu comme un classeur Excel valide.
    public static Resultat? Extraire(Stream contenu, IReadOnlyCollection<string> clesConnues, DateTime maintenant)
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
            var clesConnuesSet = clesConnues.ToHashSet();
            var valeurs = new Dictionary<string, ValeurSaisieFichier>();
            var lignesSupplementaires = new List<LigneSupplementaireCollecte>();

            foreach (var feuille in classeur.Worksheets.Where(f => !NomsOngletsExclus.Contains(f.Name)))
            {
                var poste = TrouverPoste(feuille.Name);

                foreach (var ligne in feuille.RowsUsed().Where(r => r.RowNumber() >= 4))
                {
                    var sousPoste = ligne.Cell(1).GetString().Trim();
                    var donnee = ligne.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(donnee))
                    {
                        continue;
                    }

                    string? Texte(int colonne)
                    {
                        var valeur = ligne.Cell(colonne).GetString().Trim();
                        return valeur.Length == 0 ? null : valeur;
                    }

                    var cle = poste is { } p ? $"{(int)p}|{sousPoste}|{donnee}" : null;

                    if (cle is not null && clesConnuesSet.Contains(cle))
                    {
                        // Derniere ligne gagne si le client a duplique une cle (ex. meme
                        // "Donnée à collecter" recopiee sur une ligne supplementaire).
                        valeurs[cle] = new ValeurSaisieFichier
                        {
                            LibellePersonnalise = Texte(3),
                            Valeur = Texte(4),
                            Unite = Texte(5),
                            Type = Texte(6),
                            Source = Texte(7),
                            Hypotheses = Texte(8),
                            Statut = Texte(11),
                            Commentaire = Texte(12),
                            MiseAJourLe = maintenant,
                        };
                    }
                    else
                    {
                        lignesSupplementaires.Add(new LigneSupplementaireCollecte
                        {
                            Onglet = feuille.Name,
                            SousPoste = sousPoste,
                            Donnee = donnee,
                            LibellePersonnalise = Texte(3),
                            Valeur = Texte(4),
                            Unite = Texte(5),
                            Type = Texte(6),
                            Source = Texte(7),
                            Hypotheses = Texte(8),
                            PieceJustificative = Texte(9),
                            Referent = Texte(10),
                            Statut = Texte(11),
                            Commentaire = Texte(12),
                        });
                    }
                }
            }

            return new Resultat(valeurs, lignesSupplementaires);
        }
    }

    private static PosteCollecte? TrouverPoste(string nomOnglet) =>
        Enum.GetValues<PosteCollecte>()
            .Cast<PosteCollecte?>()
            .FirstOrDefault(p => TableurCollecteGenerator.NomOnglet(p!.Value) == nomOnglet);
}
