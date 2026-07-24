using Domain.Entities;

namespace Web.Data;

public class AcceptationDocumentLegal
{
    public int Id { get; set; }

    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public int DocumentLegalId { get; set; }
    public DocumentLegal DocumentLegal { get; set; } = null!;

    public DateTime DateAcceptation { get; set; } = DateTime.UtcNow;
    public string? AdresseIp { get; set; }
}
