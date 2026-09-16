using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class TestPositionnementCarteInfo
{
    public int CarteCompetenceId { get; set; }
    public string CarteTitre { get; set; } = string.Empty;
    public int? NiveauDejaRepondu { get; set; }
}

public sealed class TestPositionnementInfo
{
    public TypeTestPositionnement Type { get; set; }
    public string ChallengeTitre { get; set; } = string.Empty;
    public List<TestPositionnementCarteInfo> Cartes { get; set; } = [];
}

public sealed class TestPositionnementCarteStat
{
    public string CarteTitre { get; set; } = string.Empty;
    public double NiveauMoyen { get; set; }
}

public sealed class TestPositionnementStats
{
    public bool Envoye { get; set; }
    public int NombreRepondants { get; set; }
    public int NombreMembresTotal { get; set; }
    public double? NiveauMoyenGlobal { get; set; }
    public List<TestPositionnementCarteStat> ParCarte { get; set; } = [];
}

// Test de connaissances amont/aval (positionnement initial + evaluation finale, exigence de
// suivi des acquis Qualiopi) - toujours une auto-evaluation par carte du Challenge (jamais
// une question a bonne/mauvaise reponse, cf. TypeTestPositionnement), envoyee par le
// Gestionnaire et jamais automatiquement.
public interface ITestPositionnementService
{
    // Idempotent : cree la campagne (Cohorte, Type) si elle n'existe pas encore, sinon
    // relance le meme email a tous les membres actuels. Amont uniquement a l'etape 1, Aval
    // uniquement a la derniere etape du Challenge.
    Task<(bool Success, string? ErrorMessage)> EnvoyerAsync(int cohorteId, TypeTestPositionnement type, string gestionnaireId, Func<int, string> construireLienReponse);

    // Renvoie null tant qu'aucune campagne n'a ete envoyee pour ce (Cohorte, Type).
    Task<TestPositionnementInfo?> GetPourReponseAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId);

    Task<bool> ADejaReponduAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId);

    // niveauxParCarte doit couvrir l'integralite des cartes du Challenge en un seul envoi -
    // jamais de reponse partielle enregistree (cf. Web.Data.TestPositionnementReponse).
    Task<(bool Success, string? ErrorMessage)> RepondreAsync(int cohorteId, TypeTestPositionnement type, string utilisateurId, Dictionary<int, int> niveauxParCarte);

    // Cote back-office : Envoye = false si la campagne n'a jamais ete lancee pour ce
    // (Cohorte, Type).
    Task<TestPositionnementStats> GetStatsAsync(int cohorteId, TypeTestPositionnement type);
}
