namespace NeoDaoBackend.Models.UserRelations;

public class UserIdWithStatusRequest : UserIdRequest
{
    public bool isFriendDeletionEnabled { get; set; }
}
