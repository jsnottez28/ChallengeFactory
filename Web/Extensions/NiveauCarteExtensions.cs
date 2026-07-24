using Domain.Entities;

namespace Web.Extensions;

public static class NiveauCarteExtensions
{
    public static string Libelle(this NiveauCarte niveau) => niveau switch
    {
        NiveauCarte.Debutant => "Débutant",
        NiveauCarte.Intermediaire => "Intermédiaire",
        NiveauCarte.Moyen => "Moyen",
        NiveauCarte.Expert => "Expert",
        _ => niveau.ToString(),
    };
}
