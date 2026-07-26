using Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.Stockage;

// Implementation par defaut de IPreuveFichierStockageService : disque local, hors
// wwwroot. Convient au developpement/a une premiere mise en production modeste ; a
// remplacer par une implementation cloud (Azure Blob / S3-compatible) le moment venu,
// sans changer IPreuveService ni les controleurs qui l'utilisent.
public class LocalDiskPreuveFichierStockageService(
    IWebHostEnvironment webHostEnvironment,
    IOptions<PreuveFichierStockageSettings> options) : IPreuveFichierStockageService
{
    private string RacineAbsolue => Path.Combine(webHostEnvironment.ContentRootPath, options.Value.RacineLocale);

    public async Task<string> EnregistrerAsync(Stream contenu, string nomFichier, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(RacineAbsolue);

        var extension = Path.GetExtension(nomFichier);
        var nomStocke = $"{Guid.NewGuid():N}{extension}";
        var cheminComplet = Path.Combine(RacineAbsolue, nomStocke);

        await using var flux = new FileStream(cheminComplet, FileMode.Create);
        await contenu.CopyToAsync(flux, cancellationToken);

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
