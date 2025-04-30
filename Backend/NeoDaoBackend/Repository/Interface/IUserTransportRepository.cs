
namespace NeoDaoBackend.Repository.Interface;

public interface IUserTransportRepository
{
    Task<bool> UserTransportExists(Guid userId, string unrealTransportId, CancellationToken ct);
    Task<List<string>> GetUserTransport(Guid userId, CancellationToken ct);
    Task AddUserTransport(Guid userId, string unrealTransportId, CancellationToken ct);
}