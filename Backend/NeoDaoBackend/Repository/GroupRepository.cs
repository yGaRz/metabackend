using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Models.Group;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public class GroupRepository : IGroupRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public GroupRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Group?> GetByIdOptional(Guid groupId, CancellationToken ct)
    {
        return await _dbContext.Groups
            .Where(g => g.GroupId == groupId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Group> GetById(Guid groupId, CancellationToken ct)
    {
        Group? group = await GetByIdOptional(groupId, ct);
        if (group == null)
        {
            throw new ApplicationException($"Group with id {groupId} not found!");
        }
        return group;
    }

    public async Task<IEnumerable<Guid>> GetAllGroupIdsForUser(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserGroups
            .Where(ug => ug.UserId == userId)
            .Select(ug => ug.GroupId)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, IGrouping<Guid, UserInGroupDTO>>> GetUsersByGroups(IEnumerable<Guid> groupIds, CancellationToken ct)
    {
        Dictionary<Guid, IGrouping<Guid, UserInGroupDTO>> response = await (
            from ug in _dbContext.UserGroups
            join g in _dbContext.Groups on ug.GroupId equals g.GroupId
            join u in _dbContext.Users on ug.UserId equals u.UserId
            where groupIds.Contains(ug.GroupId)
            orderby ug.UserStatus, ug.Created
            select new UserInGroupDTO
            {
                GroupId = ug.GroupId,
                UserId = u.UserId,
                UserName = u.UserName,
                IsLeader = g.LeaderUserId == u.UserId,
                UserStatus = ug.UserStatus,
            })
            .GroupBy(g => g.GroupId)
            .ToDictionaryAsync(pair => pair.Key, ct);
        return response;
    }

    public async Task<UserGroup> GetUserGroup(Guid userId, Guid groupId, CancellationToken ct)
    {
        return await _dbContext.UserGroups
            .Where(ug => ug.GroupId == groupId && ug.UserId == userId)
            .SingleAsync(ct);
    }

    public async Task<UserGroup?> GetActiveUserGroup(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserGroups
            .Where(ug => ug.UserId == userId && ug.UserStatus == UserGroupStatus.Active)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<List<UserGroup>> GetNotActiveUserGroup(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserGroups
            .Where(ug => ug.UserId == userId && ug.UserStatus != UserGroupStatus.Active)
            .ToListAsync(ct);
    }

    public async Task<List<UserGroup>> GetGroupMembers(Guid groupId, CancellationToken ct)
    {
        return await _dbContext.UserGroups
            .Where(ug => ug.GroupId == groupId)
            .OrderBy(ug => ug.Created)
            .ToListAsync(ct);
    }

    public async Task<List<UserGroup>> GetActiveGroupMembers(Guid groupId, CancellationToken ct)
    {
        return await _dbContext.UserGroups
            .Where(ug => ug.GroupId == groupId && ug.UserStatus == UserGroupStatus.Active)
            .OrderBy(ug => ug.Created)
            .ToListAsync(ct);
    }

    public async Task<Guid> CreateGroup(Guid userId, CancellationToken ct)
    {
        Guid groupId = Guid.NewGuid();
        Group group = new Group
        {
            LeaderUserId = userId,
            GroupId = groupId,
        };
        _dbContext.Groups.Add(group);
        UserGroup userGroup = new UserGroup
        {
            GroupId = groupId,
            UserId = userId,
            UserStatus = UserGroupStatus.Active,
        };
        _dbContext.UserGroups.Add(userGroup);
        await _dbContext.SaveChangesAsync(ct);
        return groupId;
    }

    public async Task CreateUserGroup(UserGroup userGroup, CancellationToken ct)
    {
        _dbContext.UserGroups.Add(userGroup);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateGroup(Group group, CancellationToken ct)
    {
        _dbContext.Groups.Update(group);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateUserGroup(UserGroup userGroup, CancellationToken ct)
    {
        _dbContext.UserGroups.Update(userGroup);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteGroup(Group group, CancellationToken ct)
    {
        List<UserGroup> userGroups = await GetGroupMembers(group.GroupId, ct);
        _dbContext.UserGroups.RemoveRange(userGroups);
        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync(ct);
    }

    public void DeleteUserGroupNoSave(UserGroup userGroup)
    {
        _dbContext.UserGroups.Remove(userGroup);
    }

    public async Task DeleteUserGroup(UserGroup userGroup, CancellationToken ct)
    {
        _dbContext.UserGroups.Remove(userGroup);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteUserGroupBatch(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserGroups.Where(ug => ug.UserId == userId).ExecuteDeleteAsync(ct);
    }

    public async Task DeleteNotActiveUserGroups(Guid userId, CancellationToken ct)
    {
        await _dbContext.UserGroups.Where(ug => ug.UserId == userId && ug.UserStatus != UserGroupStatus.Active).ExecuteDeleteAsync(ct);
    }

    public async Task DeleteAllGroups(CancellationToken ct)
    {
        _dbContext.UserGroups.RemoveRange(_dbContext.UserGroups);
        _dbContext.Groups.RemoveRange(_dbContext.Groups);
        await _dbContext.SaveChangesAsync( ct); 
    }


}