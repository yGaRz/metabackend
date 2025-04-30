using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public interface IUserLocationRepository
{
    Task CreateOrUpdatePlayerLocations(List<PlayerLocation> playerLocations, CancellationToken ct);
    Task<PlayerLocation?> GetPlayerLocationByUserId(Guid userId, CancellationToken ct);
}