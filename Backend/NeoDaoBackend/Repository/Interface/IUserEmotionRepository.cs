using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository.Interface;

public interface IUserEmotionRepository
{
    Task<IEnumerable<UserEmotion>> GetUserEmotions(Guid userId, CancellationToken ct);
    Task AddUserEmotion(UserEmotion emotion, CancellationToken ct);
    Task<UserEmotion?> GetUserEmotionByUnrealId(Guid userId, string unrealId, CancellationToken ct);
}