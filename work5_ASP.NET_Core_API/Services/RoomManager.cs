using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace work5_ASP.NET_Core_API.Services;

public class RoomManager : IRoomManager
{
    private record struct Connection(string Username, WebSocket Socket);
    private readonly Dictionary<string, List<Connection>> _rooms = new();

    public async Task Connect(string roomId, string username, WebSocket socket)
    {
        lock (_rooms)
        {
            if (!_rooms.ContainsKey(roomId))
                _rooms[roomId] = new List<Connection>();
            _rooms[roomId].Add(new Connection(username, socket));
        }
        await Broadcast(roomId, new { type = "system", message = $"{username} joined the room" });
    }

    public async Task Disconnect(string roomId, string username, WebSocket socket)
    {
        lock (_rooms)
        {
            if (_rooms.TryGetValue(roomId, out var connections))
            {
                connections.RemoveAll(c => c.Socket == socket);
                if (connections.Count == 0)
                    _rooms.Remove(roomId);
            }
        }
        await Broadcast(roomId, new { type = "system", message = $"{username} left the room" });
    }

    public async Task Broadcast(string roomId, object payload)
    {
        List<WebSocket> sockets;
        lock (_rooms)
        {
            if (!_rooms.TryGetValue(roomId, out var connections))
                return;
            sockets = connections.Select(c => c.Socket).ToList();
        }

        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        var buffer = new ArraySegment<byte>(bytes);

        foreach (var socket in sockets)
        {
            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public IEnumerable<string> GetUsers(string roomId)
    {
        lock (_rooms)
        {
            if (_rooms.TryGetValue(roomId, out var connections))
                return connections.Select(c => c.Username).ToList();
            return Array.Empty<string>();
        }
    }

    public void Clear()
    {
        lock (_rooms)
        {
            _rooms.Clear();
        }
    }
}