using Application.Common.Interfaces;

namespace Integration.TestSupport;

internal sealed class FakeEmailService : IEmailService
{
    public List<(string Destinataire, string Sujet, string CorpsHtml)> Envois { get; } = [];

    public Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml)
    {
        Envois.Add((destinataire, sujet, corpsHtml));
        return Task.CompletedTask;
    }
}
