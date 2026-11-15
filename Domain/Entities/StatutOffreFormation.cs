namespace Domain.Entities;

// Distingue, sur la vitrine, une formation deja montee (Disponible - presentee avec un
// lien d'inscription ou d'information reel) d'une formation qui n'existe encore que dans
// le catalogue marketing (SurDemande - CTA "nous contacter", creee seulement si un
// stagiaire ou une entreprise se manifeste, cf. CLAUDE.md).
public enum StatutOffreFormation
{
    SurDemande,
    Disponible,
}
