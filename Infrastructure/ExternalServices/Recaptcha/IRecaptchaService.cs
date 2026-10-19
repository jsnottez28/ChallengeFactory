namespace Infrastructure.ExternalServices.Recaptcha;

public interface IRecaptchaService
{
    /// <summary>
    /// Verifie auprès de Google le jeton "g-recaptcha-response" soumis par un formulaire public
    /// (ex. /contact, /accessibilite). Tant qu'aucune cle secrete n'est configuree (Recaptcha:SecretKey),
    /// la verification est desactivee et retourne toujours true, pour ne jamais bloquer un
    /// environnement sans credentials externes.
    /// </summary>
    Task<bool> ValiderAsync(string? jetonReponse, CancellationToken cancellationToken = default);
}
