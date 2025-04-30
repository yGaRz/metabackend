namespace NeoDaoBackend.Models.Auth;

public class CreateUserSessionResponse
{
    public Guid Id { get; set; }
    public string SessionUrl { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTimeOffset ExpiredAt { get; set; }
}
