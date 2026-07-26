using Domain.Entities;

namespace Application.Common.Interfaces;

// Une ligne par attribution (pas par carte) : une meme carte peut apparaitre plusieurs
// fois si elle a ete attribuee via des origines differentes (ex. Libre puis via une
// etape de Challenge) - chaque ligne porte son propre badge d'origine explicite.
public sealed class CarteBibliothequeInfo
{
    public CarteCompetence Carte { get; set; } = null!;
    public OrigineAttribution OrigineType { get; set; }
    public string? ChallengeTitre { get; set; }
    public int? NumeroEtape { get; set; }
    public DateTime AttribueLe { get; set; }
}

// Notes personnelles strictement privees (prompt "Visio planifiee par etape + notes
// personnelles sur les cartes", section 2) : ni les pairs, ni le Gestionnaire n'y ont
// jamais acces - ce ne sont pas des Preuves, pas un forum.
public sealed class CommentaireCarteInfo
{
    public int Id { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

// Cote apprenant : un utilisateur ne voit jamais que les cartes qui lui ont ete
// explicitement attribuees, jamais le catalogue complet. Ce controle doit etre applique
// cote serveur (pas seulement masque dans l'UI) - voir GetCarteAttribueeAsync qui renvoie
// null (et non la carte) si elle n'est pas attribuee a l'utilisateur demandeur. Un compte
// Suspendu/En attente de validation (statut_acces_plateforme) n'a acces a rien ici non
// plus, meme avec des attributions existantes.
public interface ICarteApprenantService
{
    Task<List<CarteBibliothequeInfo>> GetMesCartesAsync(string utilisateurId);

    Task<CarteCompetence?> GetCarteAttribueeAsync(string utilisateurId, int carteId);

    // ---- Notes personnelles sur une carte ----

    // Rattache a (Utilisateur, Carte) - pas a une attribution precise (prompt section 2.1) :
    // un meme apprenant garde le meme espace de notes quelle que soit l'origine de la
    // carte. Triees chronologiquement, jamais visibles par un autre utilisateur.
    Task<List<CommentaireCarteInfo>> GetMesCommentairesAsync(string utilisateurId, int carteId);

    // La carte doit etre attribuee a l'utilisateur (meme controle que GetCarteAttribueeAsync)
    // pour pouvoir y ajouter une note - pas d'acces libre a une carte non attribuee.
    Task<(bool Success, string? ErrorMessage, int? CommentaireId)> AjouterCommentaireAsync(string utilisateurId, int carteId, string contenu);

    // Uniquement sur ses propres commentaires (meme message d'erreur generique si le
    // commentaire n'existe pas OU appartient a quelqu'un d'autre - jamais de distinction
    // qui laisserait deviner l'existence d'un commentaire d'autrui).
    Task<(bool Success, string? ErrorMessage)> ModifierCommentaireAsync(int commentaireId, string utilisateurId, string contenu);

    Task<(bool Success, string? ErrorMessage)> SupprimerCommentaireAsync(int commentaireId, string utilisateurId);
}
