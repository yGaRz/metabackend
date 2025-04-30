using NeoDaoBackend.Models.Notification;

namespace NeoDaoBackend.Repository.Interface;

public interface INotificationRepository
{
    Task AddNotification(Notification data, CancellationToken ct);
    Task<Notification> GetNotification(CancellationToken ct);
    bool IsEmpty();
}
