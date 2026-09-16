namespace Application.Common.Interfaces;

public sealed class AttestationInfo
{
    public string ChallengeTitre { get; set; } = string.Empty;
    public string CohorteNom { get; set; } = string.Empty;
    public string? OrganisationNom { get; set; }
    public string BeneficiaireNomComplet { get; set; } = string.Empty;

    public DateTime? DateDebut { get; set; }
    public DateTime DateFin { get; set; }

    public int NombreEtapesValidees { get; set; }
    public int NombreEtapesTotal { get; set; }

    // Cartes de Competences effectivement attribuees au beneficiaire sur ce parcours (cf.
    // CarteAttribution) - independant de l'emargement, qui reste optionnel (cf. ci-dessous).
    public List<string> CartesAcquises { get; set; } = [];

    // Heures issues des Emargements signes (cf. IEmargementService) : uniquement si le
    // Gestionnaire a utilise l'emargement sur ce parcours, sinon HeuresDisponibles = false
    // et l'attestation n'affiche pas de rubrique heures plutot que d'afficher "0h".
    public bool HeuresDisponibles { get; set; }
    public decimal TotalHeuresPresence { get; set; }
    public decimal TotalHeuresTravailPersonnel { get; set; }

    public DateTime GenereeLe { get; set; } = DateTime.UtcNow;
}

// Attestation de fin de parcours (suivi de l'execution, exigence Qualiopi) : document
// derive a la volee des donnees existantes (CohorteEtapeValidation, CarteAttribution,
// Emargement) - aucune entite dediee, rien a stocker en base. Disponible uniquement une
// fois la Cohorte Terminee.
public interface IAttestationService
{
    // Renvoie null si la Cohorte n'est pas terminee ou si utilisateurId n'en est pas/plus
    // membre.
    Task<AttestationInfo?> GetAttestationAsync(int cohorteId, string utilisateurId);
}
