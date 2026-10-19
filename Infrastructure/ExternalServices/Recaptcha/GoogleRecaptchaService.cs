using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Infrastructure.ExternalServices.Recaptcha;

public class GoogleRecaptchaService(HttpClient httpClient, IOptions<RecaptchaSettings> options) : IRecaptchaService
{
    private const string UrlVerification = "https://www.google.com/recaptcha/api/siteverify";

    private readonly RecaptchaSettings _settings = options.Value;

    public async Task<bool> ValiderAsync(string? jetonReponse, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(jetonReponse))
        {
            return false;
        }

        using var contenu = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = _settings.SecretKey,
            ["response"] = jetonReponse,
        });

        using var reponse = await httpClient.PostAsync(UrlVerification, contenu, cancellationToken);
        if (!reponse.IsSuccessStatusCode)
        {
            return false;
        }

        var resultat = await reponse.Content.ReadFromJsonAsync<ReponseSiteVerify>(cancellationToken: cancellationToken);
        return resultat?.Success ?? false;
    }

    private sealed class ReponseSiteVerify
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
