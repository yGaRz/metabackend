using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.Common;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Models.UserRelations;
using NeoDaoBackend.Models.UserRelations.Search;
using NeoDaoBackend.Repository.Interface;

namespace NeoDaoBackend.Repository;

public class UserRelationsRepository : IUserRelationsRepository
{
    private readonly NeoDaoDbContext _dbContext;
    public UserRelationsRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserRelation?> GetRelationsStatus(Guid userA, Guid userB, CancellationToken ct)
    {
        return await _dbContext.UserRelations
            .Where(x => x.UserId == userA && x.RelationUserId == userB)
            .FirstOrDefaultAsync(ct);
    }

    public async Task CreateRelationsStatus(Guid userA, Guid userB,
                                FriendStatus friendStatus, BlockStatus blockStatus, CancellationToken ct)
    {
        UserRelation userRelationA = new UserRelation()
        {
            UserId = userA,
            RelationUserId = userB,
            FriendStatus = friendStatus,
            BlockStatus = blockStatus,
            Created = DateTimeOffset.UtcNow
        };
        UserRelation userRelationB = new UserRelation()
        {
            UserId = userB,
            RelationUserId = userA,
            FriendStatus = GetСomplementaryRelationStatus(friendStatus),
            BlockStatus = GetСomplementaryRelationStatus(blockStatus),
            Created = DateTimeOffset.UtcNow
        };
        _dbContext.UserRelations.Add(userRelationA);
        _dbContext.UserRelations.Add(userRelationB);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SetFriendStatus(Guid userA, Guid userB, FriendStatus friendStatus, CancellationToken ct)
    {
        var userRelationA = await GetRelationsStatus(userA, userB, ct);
        var userRelationB = await GetRelationsStatus(userB, userA, ct);
        userRelationA!.FriendStatus = friendStatus;
        _dbContext.Update(userRelationA);
        userRelationB!.FriendStatus = GetСomplementaryRelationStatus(friendStatus);
        _dbContext.Update(userRelationB);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task SetBlockStatus(Guid userA, Guid userB, BlockStatus blockStatus, CancellationToken ct)
    {
        var userRelationA = await GetRelationsStatus(userA, userB, ct);
        var userRelationB = await GetRelationsStatus(userB, userA, ct);
        userRelationA!.BlockStatus = blockStatus;
        if (blockStatus != BlockStatus.None)
        {
            userRelationA.FriendStatus = FriendStatus.None;
            userRelationB!.FriendStatus = FriendStatus.None;
        }
        _dbContext.Update(userRelationA);
        userRelationB!.BlockStatus = GetСomplementaryRelationStatus(blockStatus);
        _dbContext.Update(userRelationB);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteRelations(Guid userA, Guid userB, CancellationToken ct)
    {
        var userRelationA = await GetRelationsStatus(userA, userB, ct);
        var userRelationB = await GetRelationsStatus(userB, userA, ct);
        _dbContext.Remove(userRelationA!);
        _dbContext.Remove(userRelationB!);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<UserIdName>> GetBlockList(Guid user, CancellationToken ct)
    {
        return await _dbContext.UserRelations
                .Where(x => x.UserId == user && (x.BlockStatus == BlockStatus.OutgoingBlock || x.BlockStatus == BlockStatus.FullBlock))
                .Include(x => x.RelationUser)
                .Select(x => new UserIdName()
                {
                    Id = x.RelationUserId,
                    Name = x.RelationUser.UserName
                }
                )
                .ToListAsync(ct);
    }

    public async Task<List<UserIdName>> SearchUsers(Guid userId,
        SearchFilter filter,
        PaginationModel pagination,
        List<SortingField>? sortFields,
        CancellationToken ct)
    {
        var query = _dbContext.Users.GroupJoin(
                _dbContext.UserRelations.Where(x => x.UserId == userId),
                user => user.UserId,
                relation => relation.RelationUserId,
                (user, relation) => new { user, relation })
            .SelectMany(
                left => left.relation.DefaultIfEmpty(),
                (user, relation) => new UserIdName
                {
                    Id = user.user.UserId,
                    Name = user.user.UserName,
                    FriendStatus = relation.FriendStatus == null ? FriendStatus.None : relation.FriendStatus,
                    BlockStatus = relation.BlockStatus == null ? BlockStatus.None : relation.BlockStatus,
                    IsOnline = user.user.IsOnline
                }).Where(x => x.Id != userId);

        query = AddFiltering(query, filter);
        query = AddSorting(query, sortFields);
        query = query.Skip(pagination.Offset);
        query = query.Take(pagination.Count);
        return await query.ToListAsync(ct);

    }

    private IQueryable<UserIdName> AddFiltering(IQueryable<UserIdName> query, SearchFilter filter)
    {
        if (filter.isWithFriendRelations)
        {
            query = query.Where(x => x.FriendStatus != FriendStatus.None);
        }
        if (filter.Text != null)
        {
            query = query.Where(x => x.Name.Contains(filter.Text));
        }
        if (filter.IsOnline != null && filter.IsOnline.Value)
        {
            query = query.Where(x => x.IsOnline);
        }
        return query;
    }

    private IQueryable<UserIdName> AddSorting(IQueryable<UserIdName> query, List<SortingField>? sortFields)
    {
        if (sortFields == null)
        {
            return query;
        }
        foreach (var item in sortFields)
        {
            switch (item.Field)
            {
                case SortFieldEnum.UserName:
                    if (item.IsAsc)
                    {
                        query = query.OrderBy(x => x.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Name);
                    }
                    break;
                default:
                    throw new ApplicationException($"Unexpected sort field: {item.Field}");
            }
        }
        return query;
    }

    private FriendStatus GetСomplementaryRelationStatus(FriendStatus status) => status switch
    {
        FriendStatus.None => FriendStatus.None,
        FriendStatus.Outgoing => FriendStatus.Incoming,
        FriendStatus.Incoming => FriendStatus.Outgoing,
        _ => FriendStatus.Friend
    };

    private BlockStatus GetСomplementaryRelationStatus(BlockStatus status) => status switch
    {
        BlockStatus.None => BlockStatus.None,
        BlockStatus.IncomingBlock => BlockStatus.OutgoingBlock,
        BlockStatus.OutgoingBlock => BlockStatus.IncomingBlock,
        _ => BlockStatus.FullBlock
    };

    public async Task<List<UserIdName>> GetFriends(Guid userId, CancellationToken ct)
    {

        var query = _dbContext.Users.GroupJoin(
                _dbContext.UserRelations.Where(x => x.UserId == userId),
                user => user.UserId,
                relation => relation.RelationUserId,
                (user, relation) => new { user, relation })
            .SelectMany(
                left => left.relation.DefaultIfEmpty(),
                (user, relation) => new UserIdName
                {
                    Id = user.user.UserId,
                    Name = user.user.UserName,
                    FriendStatus = relation.FriendStatus == null ? FriendStatus.None : relation.FriendStatus,
                    BlockStatus = relation.BlockStatus == null ? BlockStatus.None : relation.BlockStatus,
                    IsOnline = user.user.IsOnline
                })
            .Where(x => x.Id != userId)
            .Where(x => x.FriendStatus != FriendStatus.None);
        return await query.ToListAsync(ct);
    }
}