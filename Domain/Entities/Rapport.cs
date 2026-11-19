namespace Domain.Entities;

// Etude de cas (Bilan Carbone(R) ou ACV) publiee sur /rapports, geree depuis le
// back-office /admin/rapports - copie du schema Article (meme logique Brouillon/Publie,
// meme rendu Markdown), sans la liste Sources : une etude de cas n'a pas de source externe
// a citer en bas de page.
public class Rapport
{
    public int Id { get; set; }

    public string Titre { get; set; } = null!;

    // Identifiant d'URL (/rapports/{Slug}), unique, genere a partir du titre mais
    // modifiable explicitement - cf. RapportService.GenererSlug.
    public string Slug { get; set; } = null!;

    public string Resume { get; set; } = null!;

    // Limitee a 160 caracteres (balise <meta name="description">) - validee cote service.
    public string MetaDescription { get; set; } = null!;

    public string ContenuMarkdown { get; set; } = null!;

    public TypeRapport Type { get; set; } = TypeRapport.BilanCarbone;

    // Nom du fichier image stocke (upload admin, deja redimensionne 1200x630 cote
    // navigateur avant envoi - cf. wwwroot/uploads/rapports). Null tant qu'aucune image
    // n'a ete deposee : la page publique se rabat alors sur le visuel de marque par
    // defaut (meme image que og:image generique dans _Layout.cshtml).
    public string? ImageCouverture { get; set; }

    // Nom du fichier PDF stocke (document original, ex. export du .docx d'etude), permet
    // un bouton "Telecharger l'etude complete" sur la fiche publique. Optionnel : un
    // rapport peut rester publie uniquement sous forme de resume en ligne.
    public string? FichierPdf { get; set; }

    public string Auteur { get; set; } = null!;

    // Date de publication, passee ou future. Un rapport Publie dont la date est encore
    // dans le futur n'apparait pas sur les pages publiques tant que cette date n'est pas
    // atteinte (filtre au moment de la requete, cf. RapportService.EstVisible) - pas de
    // tache planifiee necessaire.
    public DateTime DatePublication { get; set; }

    public StatutRapport Statut { get; set; } = StatutRapport.Brouillon;

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    public DateTime? ModifieLe { get; set; }
}
