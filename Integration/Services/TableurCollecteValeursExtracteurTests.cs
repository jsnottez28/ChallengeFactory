using Application.Common.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;
using Web.Services;

namespace Integration.Services;

public class TableurCollecteValeursExtracteurTests
{
    private static List<ElementCollecteInfo> Elements() =>
    [
        new ElementCollecteInfo { Poste = PosteCollecte.Energie, SousPoste = "Électricité", Donnee = "Consommation électricité", Unite = "kWh" },
        new ElementCollecteInfo { Poste = PosteCollecte.Energie, SousPoste = "Gaz", Donnee = "Consommation gaz naturel", Unite = "kWh" },
    ];

    private static List<string> ClesConnues(IEnumerable<ElementCollecteInfo> elements) =>
        elements.Select(e => $"{(int)e.Poste}|{e.SousPoste}|{e.Donnee}").ToList();

    [Fact]
    public void Extraire_RenvoieNull_SiLeFichierNEstPasUnClasseurExcelValide()
    {
        var resultat = TableurCollecteValeursExtracteur.Extraire(new MemoryStream("ceci n'est pas un xlsx"u8.ToArray()), [], DateTime.UtcNow);

        Assert.Null(resultat);
    }

    [Fact]
    public void Extraire_RecupereLesValeursSaisiesDesLignesReconnues()
    {
        var elements = Elements();
        var contenu = TableurCollecteGenerator.Generer("Test SAS", DateTime.UtcNow, elements);
        var rempli = SaisirCellules(contenu, "Énergie", ligne: 4, valeurs: new Dictionary<int, string>
        {
            [4] = "1250", // Valeur
            [7] = "Facture EDF janvier 2026", // Source
            [8] = "Relevé sur 11 mois, extrapolé sur 12", // Hypothèses
            [11] = "Reçu", // Statut
            [12] = "À vérifier avec le comptable", // Commentaire
        });
        var maintenant = DateTime.UtcNow;

        var resultat = TableurCollecteValeursExtracteur.Extraire(new MemoryStream(rempli), ClesConnues(elements), maintenant);

        Assert.NotNull(resultat);
        var cle = $"{(int)PosteCollecte.Energie}|Électricité|Consommation électricité";
        Assert.True(resultat.Valeurs.ContainsKey(cle));
        var valeur = resultat.Valeurs[cle];
        Assert.Equal("1250", valeur.Valeur);
        Assert.Equal("Facture EDF janvier 2026", valeur.Source);
        Assert.Equal("Relevé sur 11 mois, extrapolé sur 12", valeur.Hypotheses);
        Assert.Equal("Reçu", valeur.Statut);
        Assert.Equal("À vérifier avec le comptable", valeur.Commentaire);
        Assert.Equal(maintenant, valeur.MiseAJourLe);
        Assert.Empty(resultat.LignesSupplementaires);
    }

    [Fact]
    public void Extraire_CaptureLesLignesNonReconnuesCommeLignesSupplementaires()
    {
        var elements = Elements();
        var contenu = TableurCollecteGenerator.Generer("Test SAS", DateTime.UtcNow, elements);
        var rempli = AjouterLigne(contenu, "Énergie", ligne: 6, sousPoste: "Solaire", donnee: "Production solaire autoconsommée", valeur: "300");

        var resultat = TableurCollecteValeursExtracteur.Extraire(new MemoryStream(rempli), ClesConnues(elements), DateTime.UtcNow);

        Assert.NotNull(resultat);
        // Les 2 lignes connues restent extraites (avec des champs vides, le client ne les a
        // pas touchees) - seule la ligne ajoutee doit atterrir en ligne supplementaire.
        Assert.Equal(2, resultat.Valeurs.Count);
        var ligne = Assert.Single(resultat.LignesSupplementaires);
        Assert.Equal("Énergie", ligne.Onglet);
        Assert.Equal("Solaire", ligne.SousPoste);
        Assert.Equal("Production solaire autoconsommée", ligne.Donnee);
        Assert.Equal("300", ligne.Valeur);
    }

    private static byte[] SaisirCellules(byte[] contenu, string onglet, int ligne, Dictionary<int, string> valeurs)
    {
        using var classeur = new XLWorkbook(new MemoryStream(contenu));
        var feuille = classeur.Worksheet(onglet);
        foreach (var (colonne, valeur) in valeurs)
        {
            feuille.Cell(ligne, colonne).Value = valeur;
        }

        using var flux = new MemoryStream();
        classeur.SaveAs(flux);
        return flux.ToArray();
    }

    private static byte[] AjouterLigne(byte[] contenu, string onglet, int ligne, string sousPoste, string donnee, string valeur)
    {
        using var classeur = new XLWorkbook(new MemoryStream(contenu));
        var feuille = classeur.Worksheet(onglet);
        feuille.Cell(ligne, 1).Value = sousPoste;
        feuille.Cell(ligne, 2).Value = donnee;
        feuille.Cell(ligne, 4).Value = valeur;

        using var flux = new MemoryStream();
        classeur.SaveAs(flux);
        return flux.ToArray();
    }
}
