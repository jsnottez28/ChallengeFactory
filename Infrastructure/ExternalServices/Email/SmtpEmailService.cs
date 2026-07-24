using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.ExternalServices.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public SmtpEmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
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

        await client.SendMailAsync(message);
    }
}