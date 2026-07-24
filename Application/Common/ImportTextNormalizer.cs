using System.Text.RegularExpressions;

namespace Application.Common;

// Normalisation des cellules texte issues de data.xlsx : espaces multiples/marges
// (ex. "badge_nom" contient des retours a la ligne parasites dans le fichier reel) et
// guillemets/apostrophes typographiques ramenes a leurs equivalents droits.
public static partial class ImportTextNormalizer
{
    public static string? Normaliser(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur))
        {
            return null;
        }

        var normalise = valeur
            .Replace('‘', '\'')
            .Replace('’', '\'')
            .Replace('“', '"')
            .Replace('”', '"');

        normalise = EspacesMultiples().Replace(normalise, " ").Trim();

        return normalise.Length == 0 ? null : normalise;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacesMultiples();
}
