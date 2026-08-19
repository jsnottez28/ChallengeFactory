using Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.Stockage;

// Implementation par defaut de IPreuveFichierStockageService : disque local, hors
// wwwroot. Convient au developpement/a une premiere mise en production modeste ; a
// remplacer par une implementation cloud (Azure Blob / S3-compatible) le moment venu,
// sans changer IPreuveService ni les controleurs qui l'utilisent.
public class LocalDiskPreuveFichierStockageService(
    IWebHostEnvironment webHostEnvironment,
    IOptions<PreuveFichierStockageSettings> options,
    ILogger<LocalDiskPreuveFichierStockageService> logger) : IPreuveFichierStockageService
{
    private string RacineAbsolue => Path.Combine(webHostEnvironment.ContentRootPath, options.Value.RacineLocale);

    public async Task<string> EnregistrerAsync(Stream contenu, string nomFichier, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(nomFichier);
        var nomStocke = $"{Guid.NewGuid():N}{extension}";
        var cheminComplet = Path.Combine(RacineAbsolue, nomStocke);

        try
        {
            Directory.CreateDirectory(RacineAbsolue);

            await using var flux = new FileStream(cheminComplet, FileMode.Create);
            await contenu.CopyToAsync(flux, cancellationToken);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            // Cas type : permissions NTFS insuffisantes pour l'identite du site sur le
            // dossier de stockage (App_Data/preuves ou son parent). On journalise le detail
            // technique/chemin ici - jamais expose a l'apprenant, cf.
            // PreuveStockageIndisponibleException.
            logger.LogError(ex,
                "Echec d'ecriture du fichier de preuve sur disque local (racine : {RacineAbsolue})",
                RacineAbsolue);
            throw new PreuveStockageIndisponibleException(
                "Le stockage des fichiers de preuve est indisponible.", ex);
        }

        return nomStocke;
    }

    public Task<Stream?> TelechargerAsync(string cheminStockage, CancellationToken cancellationToken = default)
    {
        var cheminComplet = CheminSecurise(cheminStockage);
        if (cheminComplet is null || !File.Exists(cheminComplet))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream flux = new FileStream(cheminComplet, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(flux);
    }

    public Task SupprimerAsync(string cheminStockage, CancellationToken cancellationToken = default)
    {
        var cheminComplet = CheminSecurise(cheminStockage);
        if (cheminComplet is not null && File.Exists(cheminComplet))
        {
            File.Delete(cheminComplet);
        }

        return Task.CompletedTask;
    }

    // La reference stockee (CheminStockage) n'est qu'un nom de fichier genere par
    // EnregistrerAsync (jamais fourni par l'appelant) - verification defensive contre une
    // traversee de repertoire si cette hypothese venait a changer.
    private string? CheminSecurise(string cheminStockage)
    {
        var cheminComplet = Path.GetFullPath(Path.Combine(RacineAbsolue, cheminStockage));
        return cheminComplet.StartsWith(Path.GetFullPath(RacineAbsolue), StringComparison.Ordinal) ? cheminComplet : null;
    }
}
