using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.graphQL.Responses;

namespace NeoDaoBackend.Repository;

public interface IUserDataRepository
{
    Task<User?> GetByIdOptional(Guid userId, CancellationToken ct);
    Task<User> GetById(Guid userId, CancellationToken ct);
    Task<Dictionary<Guid, User>> GetUsersById(IEnumerable<Guid> userIds, CancellationToken ct);
    void CreateUserNoSave(Guid userId, string userName, CancellationToken ct);
    Task<bool> UserExists(Guid userId, CancellationToken ct);
    Task DeleteUserData(Guid userId, Guid inventoryId, CancellationToken cancellationToken);
    void UpdateIsOnlineStatus(Guid userId, bool onlineStatus);
    Task<bool> IsUserOnline(Guid userId, CancellationToken ct);
    Task UpdateUserName(Guid userId, string userName, CancellationToken ct);
    Task AddPromocodes(List<string> promocodes, CancellationToken ct);
    Task<List<string>> GetActivePromocodes(CancellationToken ct);
    Task UsePromocode(string code, CancellationToken ct);
    Task ClearPromocodes(CancellationToken ct);
    Task<int> GetCountUsesCode(string code, CancellationToken ct);
}
