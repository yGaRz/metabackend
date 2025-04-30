namespace NeoDaoBackend.Models.graphQL.Inputs;

public class AddUserSessionInput {
    public string? SuccessRedirectUrl { get; set; }
    public string? FailureRedirectUrl { get; set; }
    public string? SuccessWebhookUrl { get; set; }
    public string? FailureWebhookUrl { get; set; }
}
