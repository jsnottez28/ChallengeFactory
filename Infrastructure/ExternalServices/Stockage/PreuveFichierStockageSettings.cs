namespace Infrastructure.ExternalServices.Stockage;

public class PreuveFichierStockageSettings
{
    // Chemin relatif au content root (jamais wwwroot) ou sont ecrits les fichiers de
    // Preuve en local. Ignore si une implementation cloud est branchee a la place.
    public string RacineLocale { get; set; } = "App_Data/preuves";
}
