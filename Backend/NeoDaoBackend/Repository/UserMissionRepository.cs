using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.Mission;

namespace NeoDaoBackend.Repository;

public class UserMissionRepository : IUserMissionRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserMissionRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserMission?> GetMissionByMissionIdOptional(Guid userId, string missionId, CancellationToken ct)
    {
        return await _dbContext.UserMissions
            .Where(i => i.UserId == userId && i.MissionId == missionId).Include(x => x.Objectives)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserMission> GetMissionByMissionId(Guid userId, string missionId, CancellationToken ct)
    {
        UserMission? userMission = await GetMissionByMissionIdOptional(userId, missionId, ct);
        if (userMission == null)
        {
            throw new ApplicationException($"Cannot find user mission {missionId} for user {userId}");
        }
        return userMission;
    }

    public async Task<IEnumerable<UserMissionDTO>> GetUserMissions(Guid userId, string? missionId, MissionStatus? status, CancellationToken ct)
    {
        var query = _dbContext.UserMissions.AsQueryable();
        query = query.Where(um => um.UserId == userId).Include(x => x.Objectives);

        if (!string.IsNullOrEmpty(missionId))
        {
            query = query.Where(um => um.MissionId == missionId);
        }
        if (status.HasValue)
        {
            query = query.Where(um => um.Status == status.Value);
        }

        var missions = await query.ToListAsync(ct);
        return missions.Select(x => new UserMissionDTO()
        {
            ExpireTime = x.ExpireTime,
            MissionId = x.MissionId,
            MissionType = x.MissionType,
            Status = x.Status,
            Objectives = x.Objectives.Select(t => new ObjectiveDTO()
            {
                Metadata = t.Metadata,
                ObjectiveId = t.ObjectiveId,
                Status = t.Status
            }).ToList()
        });

    }

    public async Task CreateUserMission(Guid userId, UserMissionDTO userMissionDTO, CancellationToken ct)
    {
        var mission = new UserMission()
        {
            UserId = userId,
            MissionId = userMissionDTO.MissionId,
            ExpireTime = userMissionDTO.ExpireTime?.UtcDateTime,
            Status = userMissionDTO.Status,
            MissionType = userMissionDTO.MissionType,
            Objectives = new List<MissionObjectives>()
        };
        foreach (var objectives in userMissionDTO.Objectives)
        {
            mission.Objectives.Add(new MissionObjectives()
            {
                UserId = userId,
                MissionId = userMissionDTO.MissionId,
                ObjectiveId = objectives.ObjectiveId,
                Status = objectives.Status,
                Metadata = objectives.Metadata
            });
        }
        _dbContext.UserMissions.Add(mission);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateUserMission(Guid userId, UserMissionDTO userMissionDTO, CancellationToken ct)
    {
        var mission = await GetMissionByMissionId(userId, userMissionDTO.MissionId, ct);
        mission.Status = userMissionDTO.Status;
        mission.ExpireTime = userMissionDTO.ExpireTime?.UtcDateTime;
        foreach (var objective in userMissionDTO.Objectives)
        {
            var missionObjective = mission.Objectives.FirstOrDefault(x => x.ObjectiveId == objective.ObjectiveId);
            if (missionObjective == null)
            {
                mission.Objectives.Add(new MissionObjectives()
                {
                    UserId = userId,
                    MissionId = userMissionDTO.MissionId,
                    ObjectiveId = objective.ObjectiveId,
                    Status = objective.Status,
                    Metadata = objective.Metadata
                });

            }
            else
            {
                missionObjective.Status = objective.Status;
                missionObjective.Metadata = objective.Metadata;
            }
        }
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> MissionExists(Guid userId, string missionId, CancellationToken ct)
    {
        return await GetMissionByMissionIdOptional(userId, missionId, ct) != null;
    }
}