namespace NeoDaoBackend.Models.UserData;

public class UpdateUserNameRequest
{
    public Guid  UserId { get; set; }
    public string UserName { get; set; } = null!;
}
