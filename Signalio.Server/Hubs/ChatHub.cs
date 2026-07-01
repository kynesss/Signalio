using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Signalio.Server.Data;
using Signalio.Server.Models;
using Signalio.Server.Services;
using Signalio.Shared.Chat;

namespace Signalio.Server.Hubs;

[Authorize]
public class ChatHub(AppDbContext db, OnlineUserService onlineUsers) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
        onlineUsers.Add(Context.ConnectionId, username);

        await Clients.All.SendAsync("UserConnected", username, onlineUsers.GetAll());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
        onlineUsers.Remove(Context.ConnectionId);

        await Clients.All.SendAsync("UserDisconnected", username, onlineUsers.GetAll());
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string content)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var username = Context.User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

        var message = new Message
        {
            Content = content,
            SentAt = DateTime.UtcNow,
            UserId = userId
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync();

        var dto = new MessageDto
        {
            Username = username,
            Content = message.Content,
            SentAt = message.SentAt
        };

        await Clients.All.SendAsync("ReceiveMessage", dto);
    }
}
