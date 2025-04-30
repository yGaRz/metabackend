using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public class UserAvatarRepository : IUserAvatarRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserAvatarRepository(NeoDaoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserAvatar?> GetByUserId(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserAvatar.Where(e => e.UserId.Equals(userId)).SingleOrDefaultAsync(ct);
    }

    public async Task Create(Guid userId, AvatarGender gender, CancellationToken ct)
    {
        UserAvatar userAvatar = new UserAvatar
        {
            UserId = userId,
            Gender = gender,
        };
        _dbContext.UserAvatar.Add(userAvatar);
        await _dbContext.SaveChangesAsync(ct);
    }
}
