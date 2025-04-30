namespace NeoDaoBackend.Models.UserRelations;

public enum UserRelationEndpointKind
{
    SendFriendInvite,
    SendConfirmFriendInvite,
    DeleteFriend,
    RemoveFriendRequest,
    DeleteFriendInvite,
    SendIgnoreUser,
    DeleteIgnoreUser,
    GetBlockList,
    SearchUsers,
    GetFriends
}