namespace NeoDaoBackend.Models.graphQL.Responses;

public class AuthTokenByUserSessionRequestPayload {
    public Guid SessionRequestId { get; set; }
    public bool Success { get; set; }
    public string Token { get; set; }
}
