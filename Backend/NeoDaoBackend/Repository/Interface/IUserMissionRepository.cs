using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.Mission;

namespace NeoDaoBackend.Repository;

public interface IUserMissionRepository
{
    Task<IEnumerable<UserMissionDTO>> GetUserMissions(Guid userId, string? missionId, MissionStatus? status, CancellationToken ct);
    Task CreateUserMission(Guid userId, UserMissionDTO userMissionDTO, CancellationToken ct);
    Task UpdateUserMission(Guid userId, UserMissionDTO userMissionDTO, CancellationToken ct);
    Task<UserMission?> GetMissionByMissionIdOptional(Guid userId, string missionId, CancellationToken ct);
    Task<UserMission> GetMissionByMissionId(Guid userId, string missionId, CancellationToken ct);
    Task<bool> MissionExists(Guid userId, string missionId, CancellationToken ct);
}