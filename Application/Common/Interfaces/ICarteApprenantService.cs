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
}
