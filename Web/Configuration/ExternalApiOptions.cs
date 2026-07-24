namespace Web.Configuration;

public sealed class ExternalApiOptions
{
    public const string SectionName = "ExternalApis";

    public string GeoCommunesBaseUrl { get; set; } = "https://geo.api.gouv.fr/communes";
}
