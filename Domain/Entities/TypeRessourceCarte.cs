namespace Domain.Entities;

// Type d'une ressource attachee a une Carte de Competences (lien web, fichier PDF depose,
// ou reference bibliographique en texte libre) - cf. CarteRessource.
public enum TypeRessourceCarte
{
    LienWeb = 0,
    FichierPdf = 1,
    ReferenceBibliographique = 2,
}
