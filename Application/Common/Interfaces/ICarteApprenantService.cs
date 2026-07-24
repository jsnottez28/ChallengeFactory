using Domain.Entities;

namespace Application.Common.Interfaces;

// Cote apprenant : un utilisateur ne voit jamais que les cartes qui lui ont ete
// explicitement attribuees, jamais le catalogue complet. Ce controle doit etre applique
// cote serveur (pas seulement masque dans l'UI) - voir GetCarteAttribueeAsync qui renvoie
// null (et non la carte) si elle n'est pas attribuee a l'utilisateur demandeur.
public interface ICarteApprenantService
{
    Task<List<CarteCompetence>> GetMesCartesAsync(string utilisateurId);

    Task<CarteCompetence?> GetCarteAttribueeAsync(string utilisateurId, int carteId);
}
