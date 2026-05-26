using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using work5_ASP.NET_Core_API.Services;

namespace work5_ASP.NET_Core_API.Endpoints;

public static class WebSocketEndpoints
{
    public static void MapWebSocketEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/ws/rooms/{roomId}", async (string roomId, string? username, IRoomManager roomManager, HttpContext context) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("username required");
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await roomManager.Connect(roomId, username, socket);

            try
            {
                var buffer = new byte[4096];
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var message = JsonSerializer.Deserialize<ClientMessage>(json);

                        if (message?.Text?.Length > 300)
                        {
                            var error = new { type = "error", detail = "Message is too long" };
                            var errorJson = JsonSerializer.Serialize(error);
                            var errorBytes = Encoding.UTF8.GetBytes(errorJson);
                            await socket.SendAsync(new ArraySegment<byte>(errorBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            continue;
                        }

                        var broadcastPayload = new
                        {
                            type = "message",
                            room_id = roomId,
                            username = username,
                            text = message?.Text ?? ""
                        };
                        await roomManager.Broadcast(roomId, broadcastPayload);
                    }
                }
            }
            finally
            {
                await roomManager.Disconnect(roomId, username, socket);
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
        });

        app.MapGet("/rooms/{roomId}/users", async (string roomId, IRoomManager roomManager) =>
        {
            var users = roomManager.GetUsers(roomId);
            return Results.Ok(new { room_id = roomId, users });
        });
    }

    private class ClientMessage
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}