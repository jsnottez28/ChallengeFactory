namespace Application.Common.Interfaces;

public sealed class EmargementCarteInfo
{
    public int EmargementId { get; set; }
    public int CarteCompetenceId { get; set; }
    public string CarteTitre { get; set; } = string.Empty;
    public bool Signe { get; set; }
}

public sealed class EmargementMembreInfo
{
    public string UtilisateurId { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public List<EmargementCarteInfo> Cartes { get; set; } = [];
    public bool TousSignes => Cartes.Count > 0 && Cartes.All(c => c.Signe);
}

public sealed class EmargementPourSignatureInfo
{
    public int NumeroEtape { get; set; }
    public string ChallengeTitre { get; set; } = string.Empty;
    public List<EmargementCarteInfo> Cartes { get; set; } = [];

    // Prefill si un membre a deja partiellement signe (une seule seance -> memes heures sur
    // chaque ligne d'emargement de l'etape, cf. EmargementService.SignerAsync).
    public decimal? HeuresPresence { get; set; }
    public decimal? HeuresTravailPersonnel { get; set; }
}

// Emargement numerique par carte (tracabilite de la participation ET de l'acquisition,
// exigence de suivi de l'execution Qualiopi) - jamais de progression automatique :
// uniquement declenche par l'action explicite du Gestionnaire (bouton "Envoyer les
// emargements") puis par la signature explicite du membre.
public interface IEmargementService
{
    // Cree (idempotent) un Emargement pour chaque carte de l'etape courante attribuee a
    // chaque membre actuel de la Cohorte, puis envoie un email de demande de signature a
    // chaque membre concerne (regroupe par membre, pas un email par carte).
    Task<(bool Success, string? ErrorMessage)> EnvoyerEmargementsEtapeCouranteAsync(int cohorteId, Func<int, string> construireLienSignature);

    // Cote back-office : etat de signature de l'etape courante, par membre.
    Task<List<EmargementMembreInfo>> GetSuiviEtapeCouranteAsync(int cohorteId);

    // Cote apprenant : ce qu'il reste a signer pour l'etape courante de cette Cohorte.
    // Renvoie null tant qu'aucune demande d'emargement n'a ete envoyee pour cette etape.
    Task<EmargementPourSignatureInfo?> GetPourSignatureAsync(int cohorteId, string utilisateurId);

    // emargementIdsConfirmes : uniquement les lignes que le membre coche explicitement -
    // ne signe jamais une carte non cochee, et ne designe jamais une carte deja signee.
    Task<(bool Success, string? ErrorMessage)> SignerAsync(int cohorteId, string utilisateurId, List<int> emargementIdsConfirmes, decimal? heuresPresence, decimal? heuresTravailPersonnel);
}
