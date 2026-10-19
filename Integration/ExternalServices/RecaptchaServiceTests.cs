using System.Net;
using Infrastructure.ExternalServices.Recaptcha;
using Microsoft.Extensions.Options;

namespace Integration.ExternalServices;

public class RecaptchaServiceTests
{
    [Fact]
    public async Task ValiderAsync_LaisseToujoursPasser_SiAucuneCleSecreteConfiguree()
    {
        var service = CreerService(cleSecrete: "", reponseHttp: null);

        var resultat = await service.ValiderAsync(jetonReponse: null);

        Assert.True(resultat);
    }

    [Fact]
    public async Task ValiderAsync_Echoue_SiAucunJetonFourni_MaisCleConfiguree()
    {
        var service = CreerService(cleSecrete: "cle-secrete-test", reponseHttp: null);

        var resultat = await service.ValiderAsync(jetonReponse: null);

        Assert.False(resultat);
    }

    [Fact]
    public async Task ValiderAsync_Reussit_SiGoogleRepondSuccessTrue()
    {
        var service = CreerService(cleSecrete: "cle-secrete-test", reponseHttp: """{"success":true}""");

        var resultat = await service.ValiderAsync(jetonReponse: "jeton-valide");

        Assert.True(resultat);
    }

    [Fact]
    public async Task ValiderAsync_Echoue_SiGoogleRepondSuccessFalse()
    {
        var service = CreerService(cleSecrete: "cle-secrete-test", reponseHttp: """{"success":false}""");

        var resultat = await service.ValiderAsync(jetonReponse: "jeton-robot");

        Assert.False(resultat);
    }

    private static GoogleRecaptchaService CreerService(string cleSecrete, string? reponseHttp)
    {
        var settings = Options.Create(new RecaptchaSettings { SecretKey = cleSecrete, SiteKey = "cle-site-test" });
        var handler = new FakeHttpMessageHandler(reponseHttp);
        var httpClient = new HttpClient(handler);
        return new GoogleRecaptchaService(httpClient, settings);
    }

    private sealed class FakeHttpMessageHandler(string? corpsReponse) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var reponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(corpsReponse ?? """{"success":false}""", System.Text.Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(reponse);
        }
    }
}
