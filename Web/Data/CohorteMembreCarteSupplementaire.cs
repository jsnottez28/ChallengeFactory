using Domain.Entities;

namespace Web.Data;

// Carte personnalisee ajoutee par un Gestionnaire/Coach pour UN membre precis d'une
// Cohorte, a une etape precise - EN COMPLEMENT (jamais en remplacement) des Ressources
// Directrices partagees par toute la Cohorte (cf. ChallengeEtapeCarte), pour mieux coller
// au diagnostic propre de cette personne (bilan de competences, tests psychotechniques...).
//
// Reservee aux Challenges dont le Format == FormatChallenge.BilanCompetencesIndividuel -
// jamais aux Challenges Collectif, ou la cohesion du groupe repose sur des cartes
// partagees par toute la Cohorte (cf. CLAUDE.md, principe Manifeste "l'equipe avant
// l'individu"). La verification est faite cote service (jamais uniquement cote UI), voir
// CohorteService.DefinirCartesSupplementairesMembreAsync.
//
// Jamais de suppression silencieuse de contenu partage : ce mecanisme AJOUTE des cartes
// pour cette personne, il ne retire jamais une carte du template partage (ChallengeEtapeCarte
// reste inchange). Une ligne par (CohorteMembre, ChallengeEtape, CarteCompetence) - index
// unique pour eviter les doublons.
public class CohorteMembreCarteSupplementaire
{
    public int Id { get; set; }

    public int CohorteMembreId { get; set; }
    public CohorteMembre CohorteMembre { get; set; } = null!;

    public int ChallengeEtapeId { get; set; }
    public ChallengeEtape ChallengeEtape { get; set; } = null!;

    public int CarteCompetenceId { get; set; }
    public CarteCompetence CarteCompetence { get; set; } = null!;

    // Gestionnaire/Coach qui a ajoute cette carte - jamais l'IA (cf. CLAUDE.md, Manifeste :
    // "L'humain valide, l'IA assiste").
    public string AjouteeParId { get; set; } = string.Empty;
    public ApplicationUser AjouteePar { get; set; } = null!;

    public DateTime AjouteeLe { get; set; } = DateTime.UtcNow;
}
