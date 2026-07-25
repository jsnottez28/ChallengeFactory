using Application.Common.Interfaces;

namespace Integration.TestSupport;

// Stockage en memoire pour les tests - evite toute dependance a IWebHostEnvironment/disque
// (cf. LocalDiskPreuveFichierStockageService, l'implementation reelle).
internal sealed class FakePreuveFichierStockageService : IPreuveFichierStockageService
{
    private readonly Dictionary<string, byte[]> _fichiers = [];

    public Task<string> EnregistrerAsync(Stream contenu, string nomFichier, CancellationToken cancellationToken = default)
    {
        using var memoire = new MemoryStream();
        contenu.CopyTo(memoire);

        var cle = Guid.NewGuid().ToString("N");
        _fichiers[cle] = memoire.ToArray();

        return Task.FromResult(cle);
    }

    public Task<Stream?> TelechargerAsync(string cheminStockage, CancellationToken cancellationToken = default)
    {
        if (!_fichiers.TryGetValue(cheminStockage, out var octets))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(new MemoryStream(octets));
    }

    public Task SupprimerAsync(string cheminStockage, CancellationToken cancellationToken = default)
    {
        _fichiers.Remove(cheminStockage);
        return Task.CompletedTask;
    }
}
