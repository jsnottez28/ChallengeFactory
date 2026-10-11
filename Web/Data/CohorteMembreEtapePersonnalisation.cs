using Domain.Entities;

namespace Web.Data;

// Instructions personnalisees (Defi individuel) pour UN membre precis d'une Cohorte, a une
// etape precise - EN REMPLACEMENT, pour ce membre uniquement, du Defi individuel partage par
// le template du Challenge (ChallengeEtape.DefiIndividuel). Ce dernier reste INCHANGE en base :
// il continue de s'afficher tel quel pour toutes les autres Cohortes issues du meme Challenge
// (et pour ce membre si cette ligne est supprimee, cf. DefinirInstructionsPersonnaliseesMembreAsync).
//
// Reservee aux Challenges dont le Format == FormatChallenge.BilanCompetencesIndividuel - meme
// garde-fou que CohorteMembreCarteSupplementaire, verifie cote service (jamais uniquement cote
// UI). Une seule ligne par (CohorteMembre, ChallengeEtape) - a la difference des cartes
// personnalisees, il n'y a qu'un seul texte d'instructions possible par etape : index unique.
public class CohorteMembreEtapePersonnalisation
{
    public int Id { get; set; }

    public int CohorteMembreId { get; set; }
    public CohorteMembre CohorteMembre { get; set; } = null!;

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    public string DefiIndividuelPersonnalise { get; set; } = string.Empty;

    // Gestionnaire/Coach qui a defini ce texte - jamais l'IA (cf. CLAUDE.md, Manifeste :
    // "L'humain valide, l'IA assiste").
    public string ModifieParId { get; set; } = string.Empty;
    public ApplicationUser ModifiePar { get; set; } = null!;

    public DateTime ModifieLe { get; set; } = DateTime.UtcNow;
}
