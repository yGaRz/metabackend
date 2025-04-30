using NeoDaoBackend.Models.graphQL.Enums;

namespace NeoDaoBackend.Models.graphQL;

public class UserSession {
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiredAt { get; set; }
    public UserSessionStatus Status { get; set; }
    public string SessionUrl { get; set; }
    public string Code { get; set; }
    public User? User { get; set; }
}
