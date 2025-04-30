using NeoDaoBackend.Models.db;

namespace NeoDaoBackend.Repository;

public interface IUserAvatarRepository {
    public Task<UserAvatar?> GetByUserId(Guid userId, CancellationToken ct);
    public Task Create(Guid userId, AvatarGender gender, CancellationToken ct);
}
