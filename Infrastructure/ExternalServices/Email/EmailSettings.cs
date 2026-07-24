namespace Infrastructure.ExternalServices.Email;

public class EmailSettings
{
    public bool UseConsoleEmail { get; set; } = false;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string ExpediteurEmail { get; set; } = string.Empty;
    public string ExpediteurNom { get; set; } = string.Empty;
}