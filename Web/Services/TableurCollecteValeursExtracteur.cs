using Application.Common.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;

namespace Web.Services;

// Extrait du tableur de collecte .xlsx depose par le client les valeurs reellement
// saisies, ligne par ligne : une ligne dont la cle (Poste+Sous-poste+Donnee) correspond a
// un element connu du catalogue alimente ValeurSaisieFichier ; une ligne non reconnue
// (ex. ligne complementaire ajoutee par le client pour un second site) est capturee a part
// (cf. LigneSupplementaireCollecte), jamais perdue. Approximatif par construction : si le
// client renomme un onglet ou modifie les colonnes Sous-poste/Donnee, qui portent la cle,
// la ligne correspondante n'est plus reconnue.
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
                    var libellePersonnalise = Texte(3);
                    var valeurSaisie = Texte(4);
                    var unite = Texte(5);
                    var type = Texte(6);
                    var source = Texte(7);
                    var hypotheses = Texte(8);
                    var statut = Texte(11);
                    var commentaire = Texte(12);
                    var ligneVide = libellePersonnalise is null && valeurSaisie is null && unite is null && type is null
                        && source is null && hypotheses is null && statut is null && commentaire is null;

                    if (cle is not null && clesConnuesSet.Contains(cle))
                    {
                        // Ligne connue mais entierement vide (le client ne l'a pas encore
                        // touchee) : on ne cree pas d'entree - sinon chaque depot ecraserait
                        // silencieusement MiseAJourLe/SaisiePar de TOUTES les lignes, meme
                        // celles jamais renseignees.
                        if (ligneVide)
                        {
                            continue;
                        }

                        // Derniere ligne gagne si le client a duplique une cle (ex. meme
                        // "Donnée à collecter" recopiee sur une ligne supplementaire).
                        valeurs[cle] = new ValeurSaisieFichier
                        {
                            LibellePersonnalise = libellePersonnalise,
                            Valeur = valeurSaisie,
                            Unite = unite,
                            Type = type,
                            Source = source,
                            Hypotheses = hypotheses,
                            Statut = statut,
                            Commentaire = commentaire,
                            MiseAJourLe = maintenant,
                        };
                    }
                    else if (!ligneVide)
                    {
                        // Ligne non reconnue (cle inconnue) mais contenant au moins une
                        // donnee : ligne complementaire ajoutee par le client, a signaler.
                        // (Une ligne non reconnue ET vide - padding du modele genere, jamais
                        // touchee - est simplement ignoree.)
                        lignesSupplementaires.Add(new LigneSupplementaireCollecte
                        {
                            Onglet = feuille.Name,
                            SousPoste = sousPoste,
                            Donnee = donnee,
                            LibellePersonnalise = libellePersonnalise,
                            Valeur = valeurSaisie,
                            Unite = unite,
                            Type = type,
                            Source = source,
                            Hypotheses = hypotheses,
                            PieceJustificative = Texte(9),
                            Referent = Texte(10),
                            Statut = statut,
                            Commentaire = commentaire,
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
