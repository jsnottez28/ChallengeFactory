using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.ExternalServices.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.SmtpUser) || !string.IsNullOrWhiteSpace(_settings.SmtpPassword))
        {
            client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword);
        }

        var message = new MailMessage
        {
            From = new MailAddress(_settings.ExpediteurEmail, _settings.ExpediteurNom),
            Subject = sujet,
            Body = corpsHtml,
            IsBodyHtml = true
        };

        message.To.Add(destinataire);

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception exception)
        {
            // Sans ce log, un echec SMTP (identifiants, expediteur refuse par le
            // fournisseur...) ne laisse aucune trace exploitable a part la requete qui
            // plante - cf. session de debug sur l'envoi Gmail. On rethrow : l'appelant
            // (CollecteCarboneService, etc.) doit toujours savoir que l'envoi a echoue.
            _logger.LogError(exception, "Echec de l'envoi d'email a {Destinataire} via {SmtpHost}:{SmtpPort}",
                destinataire, _settings.SmtpHost, _settings.SmtpPort);
            throw;
        }
    }
}