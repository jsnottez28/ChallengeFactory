using Domain.Entities;

namespace Web.Extensions;

public static class TypeDocumentLegalExtensions
{
    public static string Libelle(this TypeDocumentLegal type) => type switch
    {
        TypeDocumentLegal.CGU => "Conditions générales d'utilisation",
        TypeDocumentLegal.PPD => "Politique de protection des données",
        _ => type.ToString(),
    };
}
