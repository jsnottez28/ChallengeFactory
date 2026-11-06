using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices.Email;

public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml)
    {
        // Console.WriteLine brut n'apparaissait dans aucune fenetre quand l'appli tourne
        // sous le debogueur Visual Studio (IIS Express/"Project" sans console visible) -
        // cf. session de debug sur l'envoi d'email. Passer par ILogger le rend visible
        // partout ou les autres logs de l'appli le sont deja (fenetre Sortie > Deboguage,
        // console si on lance en terminal, etc.).
        _logger.LogInformation(
            "=== Email envoyé localement ===\nTo: {Destinataire}\nSubject: {Sujet}\nBody:\n{CorpsHtml}\n===============================",
            destinataire, sujet, corpsHtml);

        return Task.CompletedTask;
    }
}
