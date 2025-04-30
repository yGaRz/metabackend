using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserRelations;
using NeoDaoBackend.Models.UserRelations.Search;

namespace NeoDaoBackend.Repository.Interface;

public interface IUserRelationsRepository
{
    public Task<UserRelation?> GetRelationsStatus(Guid userA, Guid userB, CancellationToken ct);
    public Task CreateRelationsStatus(Guid userA, Guid userB,
            FriendStatus friendStatus, BlockStatus blockStatus, CancellationToken ct);
    public Task SetFriendStatus(Guid userA, Guid userB, FriendStatus friendStatus, CancellationToken ct);
    public Task SetBlockStatus(Guid userA, Guid userB, BlockStatus blockStatus, CancellationToken ct);
    public Task DeleteRelations(Guid userA, Guid userB, CancellationToken ct);
    public Task<List<UserIdName>> GetBlockList(Guid user, CancellationToken ct);
    public Task<List<UserIdName>> SearchUsers(Guid userId,
        SearchFilter filter,
        PaginationModel pagination,
        List<SortingField>? sortFields,
        CancellationToken ct);
    public Task<List<UserIdName>> GetFriends(Guid userId,
        CancellationToken ct);
}
