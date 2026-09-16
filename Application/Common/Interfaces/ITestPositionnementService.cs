using Domain.Entities;

namespace Application.Common.Interfaces;

// Le Libelle est l'objectif pedagogique de l'etape (ChallengeEtape.ObjectifPedagogique),
// repli sur le titre de l'etape si l'objectif n'a pas ete renseigne - plus parlant pour un
// stagiaire que le titre d'une carte isolee (cf. TestPositionnementService.EtapeLibelle).
public sealed class TestPositionnementEtapeInfo
{
    public int ChallengeEtapeId { get; set; }
    public int NumeroEtape { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public int? NiveauDejaRepondu { get; set; }
}

public sealed class TestPositionnementInfo
{
    public TypeTestPositionnement Type { get; set; }
    public string ChallengeTitre { get; set; } = string.Empty;
    public List<TestPositionnementEtapeInfo> Etapes { get; set; } = [];
}

public sealed class TestPositionnementEtapeStat
{
    public int NumeroEtape { get; set; }
    public string Libelle { get; set; } = string.Empty;
    public double NiveauMoyen { get; set; }
}

public sealed class TestPositionnementStats
{
    public bool Envoye { get; set; }
    public int NombreRepondants { get; set; }
    public int NombreMembresTotal { get; set; }
    public double? NiveauMoyenGlobal { get; set; }
    public List<TestPositionnementEtapeStat> ParEtape { get; set; } = [];
}

// Test de connaissances amont/aval (positionnement initial + evaluation finale, exigence de
// suivi des acquis Qualiopi) - toujours une auto-evaluation par etape du Challenge, sur son
// objectif pedagogique (jamais une question a bonne/mauvaise reponse, cf.
// TypeTestPositionnement), envoyee par le Gestionnaire et jamais automatiquement.
public interface ITestPositionnementService
{
    // Idempotent : cree la campagne (Cohorte, Type) si elle n'existe pas encore, sinon
    // relance le meme email a tous les membres actuels. Amont uniquement a l'etape 1, Aval
    // uniquement a la derniere etape du Challenge.
    Task<(bool Success, string? ErrorMessage)> EnvoyerAsync(int cohorteId, TypeTestPositionnement type, string gestionnaireId, Func<int, string> construireLienReponse);

    // Renvoie null tant qu'aucune campagne n'a ete envoyee pour ce (Cohorte, Type).
    Task<TestPositionnementInfo?> GetPourReponseAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId);

    Task<bool> ADejaReponduAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId);

    // niveauxParEtape (cle = ChallengeEtapeId) doit couvrir l'integralite des etapes du
    // Challenge en un seul envoi - jamais de reponse partielle enregistree (cf.
    // Web.Data.TestPositionnementReponse).
    Task<(bool Success, string? ErrorMessage)> RepondreAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId, Dictionary<int, int> niveauxParEtape);

    // Cote back-office : Envoye = false si la campagne n'a jamais ete lancee pour ce
    // (Cohorte, Type).
    Task<TestPositionnementStats> GetStatsAsync(int cohorteId, TypeTestPositionnement type);
}
