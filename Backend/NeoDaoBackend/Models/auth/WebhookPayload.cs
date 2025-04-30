namespace NeoDaoBackend.Models.Auth;

public class WebhookPayload
{
    public Guid SessionRequestId { get; set; }
    public bool Success { get; set; }
    public string? Token { get; set; }
}
