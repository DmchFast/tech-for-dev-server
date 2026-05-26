using System.Net.WebSockets;

namespace work5_ASP.NET_Core_API.Services;

public interface IRoomManager
{
    Task Connect(string roomId, string username, WebSocket socket);
    Task Disconnect(string roomId, string username, WebSocket socket);
    Task Broadcast(string roomId, object payload);
    IEnumerable<string> GetUsers(string roomId);
    void Clear(); // добавить
}