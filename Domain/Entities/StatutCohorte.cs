namespace Domain.Entities;

public enum StatutCohorte
{
    // Demande d'embarquement en attente de validation humaine (cf. CLAUDE.md, aucune
    // automatisation ne doit contourner la decision du Gestionnaire) : jamais visible dans
    // le catalogue public, personne ne peut y deposer de preuve ni y demarrer une etape.
    // Place avant EnPreparation dans le cycle de vie metier, mais avec une valeur numerique
    // explicite (3, la prochaine libre) pour ne jamais decaler EnPreparation/Active/Terminee
    // deja persistes en base (stockage par defaut d'EF Core = valeur entiere de l'enum).
    Proposee = 3,
    EnPreparation = 0,
    Active = 1,
    Terminee = 2,
}
