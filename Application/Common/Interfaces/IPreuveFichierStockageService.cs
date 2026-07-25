namespace Application.Common.Interfaces;

// Abstraction de stockage des fichiers de Preuve, sur le meme principe que IEmailService
// (swappable, jamais couple en dur a l'outil choisi - cf. CLAUDE.md, "ne pas coupler
// fortement le code metier a l'outil choisi"). L'implementation par defaut de cette
// version stocke sur disque local, HORS de wwwroot (jamais servi en fichier statique
// public - une Preuve est un document potentiellement sensible, cf. CLAUDE.md
// "Stockage objet cloud pour documents sensibles"). A remplacer par une implementation
// cloud (Azure Blob / S3-compatible) sans toucher au code metier appelant.
public interface IPreuveFichierStockageService
{
    // Enregistre le contenu et retourne une reference opaque (a stocker dans
    // PreuveFichier.CheminStockage) permettant de le retrouver via TelechargerAsync.
    Task<string> EnregistrerAsync(Stream contenu, string nomFichier, CancellationToken cancellationToken = default);

    // Retourne le flux de contenu, ou null si la reference est introuvable. Ne doit
    // jamais etre exposee via une URL publique directe - toujours servie par une action
    // authentifiee qui verifie d'abord le droit d'acces a la Preuve.
    Task<Stream?> TelechargerAsync(string cheminStockage, CancellationToken cancellationToken = default);

    Task SupprimerAsync(string cheminStockage, CancellationToken cancellationToken = default);
}
