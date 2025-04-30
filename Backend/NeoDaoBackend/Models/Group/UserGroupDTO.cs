namespace NeoDaoBackend.Models.Group;

public class UserGroupDTO
{
    public Guid GroupId { get; set; }
    public List<UserInGroupDTO> Users { get; set; } = null!;
}
