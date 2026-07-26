namespace Web.Data;

// Le controle "marqueur != auteur du message" est applique cote serveur dans
// IForumService. Un meme (MessageId, MarqueParId) ne peut apparaitre qu'une fois (index
// unique).
public class ForumMessageUtile
{
    public int Id { get; set; }

    public int MessageId { get; set; }
    public ForumMessage Message { get; set; } = null!;

    public string MarqueParId { get; set; } = string.Empty;
    public ApplicationUser MarquePar { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.UtcNow;
}
