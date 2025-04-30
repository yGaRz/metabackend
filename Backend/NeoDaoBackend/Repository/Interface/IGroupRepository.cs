using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.Group;

namespace NeoDaoBackend.Repository.Interface;

public interface IGroupRepository
{
    Task<Group?> GetByIdOptional(Guid groupId, CancellationToken ct);
    Task<Group> GetById(Guid groupId, CancellationToken ct);
    Task<IEnumerable<Guid>> GetAllGroupIdsForUser(Guid userId, CancellationToken ct);
    Task<Dictionary<Guid, IGrouping<Guid, UserInGroupDTO>>> GetUsersByGroups(IEnumerable<Guid> groupIds, CancellationToken ct);
    Task<UserGroup> GetUserGroup(Guid userId, Guid groupId, CancellationToken ct);
    Task<UserGroup?> GetActiveUserGroup(Guid userId, CancellationToken ct);
    Task<List<UserGroup>> GetNotActiveUserGroup(Guid userId, CancellationToken ct);
    Task<List<UserGroup>> GetActiveGroupMembers(Guid groupId, CancellationToken ct);
    Task<List<UserGroup>> GetGroupMembers(Guid groupId, CancellationToken ct);

    Task<Guid> CreateGroup(Guid userId, CancellationToken ct);
    Task CreateUserGroup(UserGroup userGroup, CancellationToken ct);
    Task UpdateGroup(Group group, CancellationToken ct);
    Task UpdateUserGroup(UserGroup userGroup, CancellationToken ct);
    Task DeleteGroup(Group group, CancellationToken ct);
    void DeleteUserGroupNoSave(UserGroup userGroup);
    Task DeleteUserGroup(UserGroup userGroup, CancellationToken ct);
    Task DeleteNotActiveUserGroups(Guid userId, CancellationToken ct);
    Task<int> DeleteUserGroupBatch(Guid userId, CancellationToken ct);
    Task DeleteAllGroups(CancellationToken ct);
}
