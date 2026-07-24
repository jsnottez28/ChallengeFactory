using Domain.Entities;

namespace Application.Common.Interfaces;

public sealed class DocumentLegalInput
{
    public TypeDocumentLegal Type { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateEffective { get; set; }
}

public interface IDocumentLegalService
{
    /// <summary>Toutes les versions d'un type de document, la plus recente en premier.</summary>
    Task<List<DocumentLegal>> GetHistoriqueAsync(TypeDocumentLegal type);

    /// <summary>Version actuellement publiee (EstPublie = true) pour un type de document, s'il en existe une.</summary>
    Task<DocumentLegal?> GetVersionPublieeAsync(TypeDocumentLegal type);

    /// <summary>Cree une nouvelle version et la publie immediatement ; depublie l'ancienne version du meme type.</summary>
    Task<(bool Success, string? ErrorMessage, DocumentLegal? Document)> CreerEtPublierAsync(DocumentLegalInput input);

    /// <summary>Types de documents pour lesquels l'utilisateur n'a pas accepte la version actuellement publiee.</summary>
    Task<List<TypeDocumentLegal>> GetTypesEnAttenteAsync(string applicationUserId);

    /// <summary>
    /// Parmi les types en attente, ceux dont la publication est anterieure au debut de la session en cours
    /// (l'utilisateur se connecte alors que la nouvelle version est deja en vigueur) : ceux-la seuls bloquent
    /// l'acces. Les autres (deja connecte au moment de la publication) sont a signaler sans bloquer.
    /// </summary>
    Task<List<TypeDocumentLegal>> GetTypesBloquantsAsync(string applicationUserId, DateTimeOffset sessionDebutUtc);

    Task<(bool Success, string? ErrorMessage)> AccepterAsync(string applicationUserId, TypeDocumentLegal type, string? adresseIp);
}
