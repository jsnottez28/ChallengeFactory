namespace Application.Common.Interfaces;

// Levee par une implementation de IPreuveFichierStockageService quand l'ecriture/lecture
// technique echoue (permissions disque, service cloud indisponible, etc.). Permet a
// PreuveService de distinguer un echec de stockage (a annoncer proprement a l'apprenant,
// sans exposer de detail technique/chemin serveur) d'une exception imprevue qui doit
// continuer a remonter en 500. L'implementation qui la leve est responsable de logger le
// detail technique original avant de la lever (cf. LocalDiskPreuveFichierStockageService).
public sealed class PreuveStockageIndisponibleException(string message, Exception innerException)
    : Exception(message, innerException);
