namespace Domain.Entities;

public enum OrigineAttribution
{
    // Creee automatiquement par la validation d'une etape de Cohorte (ICohorteService).
    Challenge,

    // Creee manuellement depuis l'admin Cartes (moteur de cartes existant).
    Libre
}
