using NeoDaoBackend.Models.Notification;
using NeoDaoBackend.Repository.Interface;
using System.Collections.Concurrent;

namespace NeoDaoBackend.Repository;

public class NotificationRepositoryInMemory : INotificationRepository
{
    private static ConcurrentQueue<Notification> _notificationsQueue = new ConcurrentQueue<Notification>();

    public async Task AddNotification(Notification data, CancellationToken ct)
    {
        _notificationsQueue.Enqueue(data);
    }

    public async Task<Notification> GetNotification(CancellationToken ct)
    {
        _notificationsQueue.TryDequeue(out var notification);
        return notification!;
    }

    public bool IsEmpty()
    {
        return _notificationsQueue.IsEmpty;
    }
}
