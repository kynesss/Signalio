namespace Signalio.Server.Models;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;
}
