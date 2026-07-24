namespace Domain.Entities;

// Carte de Competences ("Carte Apprenante" en interne). Une carte = 2 faces :
// Theorie (identite/objectifs) et Defi (mise en pratique terrain).
// Champs alignes sur data.xlsx (feuille "cartes") pour rester compatibles avec
// la reprise/l'import existant - ne pas renommer sans raison.
public class CarteCompetence
{
    public int Id { get; set; }

    // ---- Face Theorie ----
    public string Code { get; set; } = null!;

    public int? BadgeId { get; set; }
    public Badge? Badge { get; set; }

    public NiveauCarte Niveau { get; set; }
    public string TitreTheorie { get; set; } = null!;

    public string? Objectif1 { get; set; }
    public string? Objectif2 { get; set; }
    public string? Objectif3 { get; set; }
    public string? Objectif4 { get; set; }

    public string? Citation { get; set; }
    public string? AuteurCitation { get; set; }

    // Nom du fichier image stocke (upload admin ou import Excel).
    public string? ImageCarteA { get; set; }

    // ---- Face Defi ----
    public string? TitreDefi { get; set; }
    public string? ContextePro { get; set; }
    public string? ContextePerso { get; set; }
    public string? TonDefi { get; set; }

    public string? Etape1 { get; set; }
    public string? Etape2 { get; set; }
    public string? Etape3 { get; set; }
    public string? Etape4 { get; set; }
    public string? Etape5 { get; set; }

    public string? Tip1 { get; set; }
    public string? Tip2 { get; set; }
    public string? Tip3 { get; set; }
    public string? Tip4 { get; set; }
    public string? Tip5 { get; set; }

    public string? CitationHumour { get; set; }
    public string? LienVideo { get; set; }

    public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    public DateTime? ModifieLe { get; set; }
}
