using Domain.Entities;

namespace Web.Data;

// Point d'etape a mi-parcours (methodologie bilan de competences, phase Investigation) :
// envoye automatiquement a tous les membres d'une Cohorte des que celle-ci atteint son
// etape mediane (cf. CohorteService.EnvoyerQuestionnaireMiParcoursSiEtapeMedianeAsync).
// Generique, pas reserve au bilan de competences (aucune branche par type de Challenge) -
// simplement inutile pour un Challenge a une seule etape, ou personne ne sera jamais
// sollicite (cf. le meme service).
public class QuestionnaireMiParcoursReponse
{
    public int Id { get; set; }

    public int CohorteId { get; set; }
    public Cohorte Cohorte { get; set; } = null!;

    public string UtilisateurId { get; set; } = string.Empty;
    public ApplicationUser Utilisateur { get; set; } = null!;

    public PertinenceProjet ProjetToujoursPertinent { get; set; }

    // 0 (pas du tout satisfait) a 10 (tout a fait satisfait) - meme echelle que
    // SatisfactionReponse.Score, pour rester coherent avec les autres enquetes.
    public int NoteAccompagnement { get; set; }

    public string? DifficultesRencontrees { get; set; }
    public string? AjustementsSouhaites { get; set; }

    public DateTime RepondueLe { get; set; } = DateTime.UtcNow;
}
