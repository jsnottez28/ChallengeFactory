namespace Domain.Entities;

// Ressource documentaire attachee a une Carte de Competences, en complement du contenu de
// la carte elle-meme (Citation/LienVideo deja presents sur CarteCompetence restent pour le
// contenu editorial de la carte ; CarteRessource couvre les ressources "pour aller plus
// loin" ajoutees librement par un gestionnaire : lien web, PDF depose, ou reference
// bibliographique). Une carte peut en avoir plusieurs, affichees dans l'ordre Ordre croissant
// puis Id (cf. CarteCompetenceService).
public class CarteRessource
{
    public int Id { get; set; }

    public int CarteCompetenceId { get; set; }
    public CarteCompetence CarteCompetence { get; set; } = null!;

    public TypeRessourceCarte Type { get; set; }

    // Libelle affiche a l'apprenant, obligatoire quel que soit le type (ex. "Article sur
    // l'écoute active" pour un lien, "Fiche technique OSBD" pour un PDF).
    public string Titre { get; set; } = null!;

    // Rempli selon Type, les deux autres restant null - cf. CarteCompetenceService pour la
    // validation (un seul des trois doit etre renseigne a la creation).
    public string? Url { get; set; } // Type = LienWeb
    public string? NomFichier { get; set; } // Type = FichierPdf : nom du fichier stocke sur disque (meme principe que CarteCompetence.ImageCarteA)
    public string? ReferenceTexte { get; set; } // Type = ReferenceBibliographique : citation libre (auteur, titre, annee, editeur...)

    public int Ordre { get; set; }

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
}
