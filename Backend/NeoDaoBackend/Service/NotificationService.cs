using NeoDaoBackend.Metrics;
using NeoDaoBackend.Middlewares;
using NeoDaoBackend.Models.Notification;
using NeoDaoBackend.Models.WsMessage;
using NeoDaoBackend.Repository.Interface;
using NeoDaoBackend.Util;
using Newtonsoft.Json;
using System.Collections.Frozen;
using System.Net.WebSockets;

namespace NeoDaoBackend.Service;

public class NotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationService> _logger;
    private readonly WebSocketConnectionManager _connectionManager;
    private readonly JsonSerializerSettings _jsonSerializer;
    private readonly UserWSMetrics _userWSMetrics;

    public NotificationService(WebSocketConnectionManager connectionManager,
        ILogger<NotificationService> logger,
        INotificationRepository notificationRepository,
        JsonSerializerSettings jsonSerializer,
        UserWSMetrics userWSMetrics)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
        _connectionManager = connectionManager;
        _jsonSerializer = jsonSerializer;
        _userWSMetrics = userWSMetrics;
    }

    public async Task AddNotificationToAllUsers(IOutputMessageData? messageData, string eventCategory, string eventType, CancellationToken ct)
    {
        Notification notification = new Notification()
        {
            Message = messageData,
            RecipientId = null,
            EventCategory = eventCategory,
            EventType = eventType,
            ToAllUser = true
        };
        await _notificationRepository.AddNotification(notification, ct);
    }

    public async Task AddNotificationToSingleUser(Guid recipientId, IOutputMessageData? messageData, string eventCategory, string eventType, CancellationToken ct)
    {
        Notification notification = new Notification()
        {
            Message = messageData,
            RecipientId = recipientId,
            EventCategory = eventCategory,
            EventType = eventType,
            ToAllUser = false
        };
        await _notificationRepository.AddNotification(notification, ct);
    }

    public async Task NotificationCycle(CancellationToken ct)
    {
        while (!_notificationRepository.IsEmpty())
        {
            var notification = await _notificationRepository.GetNotification(ct);
            if (notification.ToAllUser)
            {
                await SendUserMessageToAllUser(notification, ct);
            }
            else
            {
                await SendMessageToSingleUser(notification, ct);
            }
        }
    }

    private async Task SendMessageToSingleUser(Notification notification, CancellationToken ct)
    {
        var ws = _connectionManager.GetSocket(notification.RecipientId!.Value);
        if (ws != null && ws.State == WebSocketState.Open)
        {
            _userWSMetrics.AddOutputWebSocketMessage();
            await WebSocketUtils.SendMessageToUser(ws, notification.RecipientId.Value, _logger, notification.EventType, notification.EventCategory, notification.Message, _jsonSerializer, ct);
        }
    }

    private async Task SendUserMessageToAllUser(Notification notification, CancellationToken ct)
    {
        List<Task> tasks = [];
        FrozenDictionary<Guid, WebSocket> userIdToSocket = _connectionManager.GetUserIdToSocket();
        foreach (var webSocket in userIdToSocket)
        {
            Guid recipientUserId = webSocket.Key;
            if (webSocket.Value.State == WebSocketState.Open)
            {
                _userWSMetrics.AddOutputWebSocketMessage();
                tasks.Add(WebSocketUtils.SendMessageToUser(webSocket.Value, webSocket.Key, _logger, notification.EventType, notification.EventCategory, notification.Message, _jsonSerializer, ct));
            }
        }
        await Task.WhenAll(tasks);
    }
}
