using Domain.Entities;

namespace Web.Data;

// Attribution d'une Carte de Competences a un utilisateur (many-to-many avec
// tracabilite : qui a attribue, quand, dans quel contexte). Vit dans Web.Data (et pas
// Domain.Entities) car elle porte des references directes vers ApplicationUser, comme
// AcceptationDocumentLegal/Rattachement/Scope.
public class CarteAttribution
{
    public int Id { get; set; }

    public int CarteCompetenceId { get; set; }
    public CarteCompetence CarteCompetence { get; set; } = null!;

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    // Gestionnaire/Coach qui a realise l'attribution (jamais l'IA, cf. Manifeste :
    // l'attribution de cartes est un outil de pilotage pedagogique humain).
    public string AttribueParId { get; set; } = string.Empty;
    public ApplicationUser AttribuePar { get; set; } = null!;

    public DateTime AttribueLe { get; set; } = DateTime.UtcNow;

    // Optionnel : rattachement a un Challenge/une cohorte precis, ou attribution libre.
    public string? Contexte { get; set; }

    // Desattribution = bascule a false plutot que suppression physique, pour conserver
    // la tracabilite (qui a attribue/desattribue, quand) - meme principe que
    // Rattachement.EstActif / Scope.EstActif.
    public bool EstActif { get; set; } = true;
}
