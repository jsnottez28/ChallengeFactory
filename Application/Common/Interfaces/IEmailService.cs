namespace Application.Common.Interfaces;

public interface IEmailService
{
    Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml);
}