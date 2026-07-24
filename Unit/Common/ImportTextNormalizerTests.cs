using Application.Common;

namespace Unit.Common;

public class ImportTextNormalizerTests
{
    [Fact]
    public void Normaliser_RenvoieNull_QuandValeurVideOuBlanche()
    {
        Assert.Null(ImportTextNormalizer.Normaliser(null));
        Assert.Null(ImportTextNormalizer.Normaliser(""));
        Assert.Null(ImportTextNormalizer.Normaliser("   "));
    }

    [Fact]
    public void Normaliser_ReduitLesEspacesMultiplesEtLesRetoursALaLigne()
    {
        var resultat = ImportTextNormalizer.Normaliser("  Management  du\n changement  ");

        Assert.Equal("Management du changement", resultat);
    }

    [Fact]
    public void Normaliser_RemplaceLesGuillemetsTypographiquesParDesGuillemetsDroits()
    {
        var resultat = ImportTextNormalizer.Normaliser("L’équipe dit “oui”");

        Assert.Equal("L'équipe dit \"oui\"", resultat);
    }

    [Fact]
    public void Normaliser_ConserveUneValeurDejaPropre()
    {
        var resultat = ImportTextNormalizer.Normaliser("MAN-C23");

        Assert.Equal("MAN-C23", resultat);
    }
}
