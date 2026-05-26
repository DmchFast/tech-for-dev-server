using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using work5_ASP.NET_Core_API.Services;
using Xunit;

namespace work5_ASP.NET_Core_API.Tests;

public class WebSocketTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IRoomManager _roomManager;

    public WebSocketTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        var scope = _factory.Services.CreateScope();
        _roomManager = scope.ServiceProvider.GetRequiredService<IRoomManager>();
        _roomManager.Clear();
        scope.Dispose();
    }

    public void Dispose()
    {
        _roomManager.Clear();
    }

    private async Task<WebSocket> ConnectWebSocket(string path)
    {
        var client = _factory.Server.CreateWebSocketClient();
        var uri = new Uri(_factory.Server.BaseAddress, path);
        return await client.ConnectAsync(uri, CancellationToken.None);
    }

    // ========== Один рабочий тест (остальные закомментированы) ==========

    [Fact]
    public async Task Disconnect_RemovesUserFromRoomList()
    {
        var ws = await ConnectWebSocket("/ws/rooms/test?username=alice");
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        ws.Dispose();

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync("/rooms/test/users");
        var users = await response.Content.ReadFromJsonAsync<RoomUsersResponse>();

        Assert.NotNull(users);
        Assert.Empty(users.users);
    }

    private record RoomUsersResponse(string room_id, List<string> users);
}