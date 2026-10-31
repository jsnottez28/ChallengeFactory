namespace Domain.Entities;

// Un lien "source" affiche en bas de page d'article (cf. le bloc "Sources :" deja utilise
// sur les articles statiques du Lab). Libelle optionnel : si absent, l'URL elle-meme sert
// de texte de lien.
public class ArticleSource
{
    public int Id { get; set; }

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public string? Libelle { get; set; }
    public string Url { get; set; } = null!;

    public int Ordre { get; set; }
}
