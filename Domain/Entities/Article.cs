namespace Domain.Entities;

// Article de blog publie sur /articles, gere depuis le back-office /admin/articles. Le
// contenu est saisi en Markdown et rendu en HTML a l'affichage (jamais stocke en HTML -
// cf. ArticleService.RendreHtml, pipeline Markdig avec DisableHtml() pour ne jamais
// laisser passer de balise brute saisie dans le Markdown source).
public class Article
{
    public int Id { get; set; }

    public string Titre { get; set; } = null!;

    // Identifiant d'URL (/articles/{Slug}), unique, genere a partir du titre mais
    // modifiable explicitement - cf. ArticleService.GenererSlug.
    public string Slug { get; set; } = null!;

    public string Resume { get; set; } = null!;

    // Limitee a 160 caracteres (balise <meta name="description">) - validee cote service.
    public string MetaDescription { get; set; } = null!;

    public string ContenuMarkdown { get; set; } = null!;

    // Nom du fichier image stocke (upload admin, deja redimensionne 1200x630 cote
    // navigateur avant envoi - cf. wwwroot/uploads/articles). Null tant qu'aucune image
    // n'a ete deposee : la page publique se rabat alors sur le visuel de marque par
    // defaut (meme image que og:image generique dans _Layout.cshtml).
    public string? ImageCouverture { get; set; }

    public string Auteur { get; set; } = null!;

    // Date de publication, passee ou future. Un article Publie dont la date est encore
    // dans le futur n'apparait pas sur les pages publiques/RSS tant que cette date n'est
    // pas atteinte (filtre au moment de la requete, cf. ArticleService.EstVisible) - pas
    // de tache planifiee necessaire.
    public DateTime DatePublication { get; set; }

    public StatutArticle Statut { get; set; } = StatutArticle.Brouillon;

    // Liens sources cites en bas d'article - toujours charge explicitement (.Include) par
    // le service, jamais via lazy loading (meme principe que CarteCompetence.Ressources).
    public List<ArticleSource> Sources { get; set; } = [];

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    public DateTime? ModifieLe { get; set; }
}
