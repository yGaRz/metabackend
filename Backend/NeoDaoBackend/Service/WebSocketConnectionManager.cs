namespace NeoDaoBackend.Middlewares;

using NeoDaoBackend.Metrics;
using NeoDaoBackend.Models.Auth;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Net.WebSockets;

public class WebSocketConnectionManager(UserWSMetrics userWSMetrics)
{
    private readonly UserWSMetrics _userWSMetrics=userWSMetrics;
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new ConcurrentDictionary<Guid, WebSocket>();

    public string? AddSocket(WebSocket socket, NeoDaoUser user)
    {
        if (user.UserName == null)
        {
            return "Illegal state: trying to open webSocket for a user without a username";
        }
        if (user.UserId == null)
        {
            return "Illegal state: trying to open webSocket for a user without a user id";
        }
        _sockets.TryAdd(user.UserId.Value, socket);
        _userWSMetrics.WebSocketConnectionUpdate(_sockets.Count);
        return null;
    }

    public WebSocket? GetSocket(Guid userId)
    {
        _sockets.TryGetValue(userId, out var socket);
        return socket;
    }

    public FrozenDictionary<Guid, WebSocket> GetUserIdToSocket()
    {
        return _sockets.ToFrozenDictionary();
    }

    public void RemoveSocket(Guid userId)
    {
        _sockets.TryRemove(userId, out _);
        _userWSMetrics.WebSocketConnectionUpdate(_sockets.Count);
    }
}

