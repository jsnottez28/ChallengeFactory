namespace Domain.Entities;

// Cf. CLAUDE.md, "La preuve remplace le QCM" : une Preuve est LE mecanisme de
// validation de competence, jamais un QCM. Le statut refuse a chaud tant que le
// Gestionnaire n'a pas cloture l'etape (voir ICohorteService.ValiderEtapeAsync) - voir
// IPreuveService pour les regles de transition exactes (seuil pairs, figeage).
public enum StatutPreuve
{
    Soumise,
    ValideeParLesPairs,
    ValideeDefinitivement,
    NonValideeALaCloture,
}
