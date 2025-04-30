namespace NeoDaoBackend.Models.Auth;

public class AuthorizeFakeUserRequest
{
    public Guid userId { get; set; }
    public string? userName { get; set; }  
}
