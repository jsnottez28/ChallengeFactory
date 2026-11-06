using Application.Common.Interfaces;
using Domain.Entities;
using Web.Services;

namespace Integration.Services;

public class TableurCollecteAvancementCalculatorTests
{
    private static List<ElementCollecteInfo> Elements(int nombreConnus, int nombreInconnus) =>
    [
        .. Enumerable.Range(0, nombreConnus).Select(i => new ElementCollecteInfo
        {
            Poste = PosteCollecte.Energie,
            Donnee = $"Connu {i}",
            Unite = "kWh",
            Valeur = 100m,
        }),
        .. Enumerable.Range(0, nombreInconnus).Select(i => new ElementCollecteInfo
        {
            Poste = PosteCollecte.Energie,
            Donnee = $"Inconnu {i}",
            Unite = "kWh",
        }),
    ];

    [Fact]
    public void Calculer_CompteLesLignesRecuesSurLeTotal()
    {
        var contenu = TableurCollecteGenerator.Generer("Test SAS", DateTime.UtcNow, Elements(nombreConnus: 3, nombreInconnus: 1));

        var avancement = TableurCollecteAvancementCalculator.Calculer(new MemoryStream(contenu));

        Assert.Equal(75, avancement); // 3 "Reçu" (Valeur pré-remplie) sur 4 lignes au total
    }

    [Fact]
    public void Calculer_RenvoieZero_SiAucuneValeurConnue()
    {
        var contenu = TableurCollecteGenerator.Generer("Test SAS", DateTime.UtcNow, Elements(nombreConnus: 0, nombreInconnus: 5));

        var avancement = TableurCollecteAvancementCalculator.Calculer(new MemoryStream(contenu));

        Assert.Equal(0, avancement);
    }

    [Fact]
    public void Calculer_RenvoieNull_SiLeFichierNEstPasUnClasseurExcelValide()
    {
        var avancement = TableurCollecteAvancementCalculator.Calculer(new MemoryStream("ceci n'est pas un xlsx"u8.ToArray()));

        Assert.Null(avancement);
    }
}
