using Domain.Entities;

namespace Web.Data;

// Notes personnelles d'un apprenant sur une Carte de Competences - strictement privees
// (ni les pairs, ni le Gestionnaire n'y ont acces : ce ne sont pas des Preuves, pas un
// forum). Rattachees a (Utilisateur, Carte), pas a une attribution precise : un meme
// apprenant garde le meme espace de notes quelle que soit l'origine de la carte. Un
// apprenant peut en accumuler plusieurs sur la meme carte (journal chronologique), pas
// un champ unique ecrase a chaque modification.
public class CommentaireCarte
{
    public int Id { get; set; }

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public int CarteCompetenceId { get; set; }
    public CarteCompetence CarteCompetence { get; set; } = null!;

    public string Contenu { get; set; } = string.Empty;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
}
