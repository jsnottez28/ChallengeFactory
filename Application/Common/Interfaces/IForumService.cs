namespace Application.Common.Interfaces;

public sealed class ForumMessageInfo
{
    public int Id { get; set; }
    public string AuteurNomComplet { get; set; } = string.Empty;
    public bool EstAuteur { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public int NombreUtile { get; set; }
    public bool MarqueUtileParMoi { get; set; }
    public List<ForumMessageInfo> Reponses { get; set; } = [];
}

public sealed class EtapeForumInfo
{
    public int ChallengeEtapeId { get; set; }
    public int NumeroEtape { get; set; }
    public string TitreEtape { get; set; } = string.Empty;
    public bool EstEtapeCourante { get; set; }
}

public interface IForumService
{
    // Etapes deja atteintes par la Cohorte (courante incluse) : navigation entre le
    // forum de l'etape courante et l'historique des etapes precedentes (jamais purge).
    // estAdmin=true (droit FORUM.SUPPRIMER, cf. controleur) contourne le controle de
    // membership - un Gestionnaire moderant le forum n'est pas necessairement membre de
    // la Cohorte.
    Task<List<EtapeForumInfo>> GetEtapesAccessiblesAsync(int cohorteId, string utilisateurId, bool estAdmin = false);

    Task<List<ForumMessageInfo>> GetMessagesEtapeAsync(int challengeEtapeId, int cohorteId, string utilisateurId, bool estAdmin = false);

    // Refuse si l'etape n'est pas l'etape courante de la Cohorte (les forums d'etapes
    // passees restent lisibles mais ne recoivent plus de nouveaux messages, cf. prompt
    // section 6, "historique"). lienForum (construit par l'appelant, cf. Url.Page - le
    // service ne construit jamais d'URL lui-meme) sert aux notifications in-app generees :
    // message racine -> tous les autres membres de la cohorte ; reponse -> uniquement
    // l'auteur du message parent (evite une double notification pour le meme evenement).
    Task<(bool Success, string? ErrorMessage)> PosterMessageAsync(
        string auteurId, int cohorteId, int challengeEtapeId, string contenu, int? messageParentId, string lienForum);

    // Refuse si marqueParId == auteur du message (controle serveur). Genere des
    // Points_Karma et une notification in-app pour l'auteur du message, une seule fois
    // par (message, marqueur).
    Task<(bool Success, string? ErrorMessage)> MarquerUtileAsync(int messageId, string marqueParId, string lienForum);

    // Moderation (droit FORUM.SUPPRIMER) : supprime le message et ses reponses en fil.
    Task<(bool Success, string? ErrorMessage)> SupprimerMessageAsync(int messageId);
}
