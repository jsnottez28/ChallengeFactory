using Application.Common.Interfaces;

namespace Infrastructure.ExternalServices.Email;

public class ConsoleEmailService : IEmailService
{
    public Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml)
    {
        Console.WriteLine("=== Email envoyé localement ===");
        Console.WriteLine($"To: {destinataire}");
        Console.WriteLine($"Subject: {sujet}");
        Console.WriteLine("Body:");
        Console.WriteLine(corpsHtml);
        Console.WriteLine("===============================");

        return Task.CompletedTask;
    }
}
