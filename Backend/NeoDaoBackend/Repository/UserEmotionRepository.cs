using Microsoft.EntityFrameworkCore;
using NeoDaoBackend.Models;
using NeoDaoBackend.Models.db;
using NeoDaoBackend.Repository.Interface;

namespace NeoDaoBackend.Repository;

public class UserEmotionRepository: IUserEmotionRepository
{
    private readonly NeoDaoDbContext _dbContext;

    public UserEmotionRepository(NeoDaoDbContext context)
    {
        _dbContext = context;
    }

    public async Task<IEnumerable<UserEmotion>> GetUserEmotions(Guid userId, CancellationToken ct)
    {
        return await _dbContext.UserEmotions
            .Where(ue => ue.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task AddUserEmotion(UserEmotion emotion, CancellationToken ct)
    {
        _dbContext.UserEmotions.Add(emotion);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<UserEmotion?> GetUserEmotionByUnrealId(Guid userId, string unrealId, CancellationToken ct)
    {
        return await _dbContext.UserEmotions.FirstOrDefaultAsync(x => x.UserId == userId && x.UnrealId == unrealId, cancellationToken: ct);
    }
}