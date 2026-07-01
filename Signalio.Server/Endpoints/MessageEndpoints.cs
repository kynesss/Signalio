using Microsoft.EntityFrameworkCore;
using Signalio.Server.Data;
using Signalio.Shared.Chat;

namespace Signalio.Server.Endpoints;

public static class MessageEndpoints
{
    private const int DefaultTake = 50;
    private const int MaxTake = 100;

    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/messages", GetMessages).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetMessages(AppDbContext db, int take = DefaultTake)
    {
        take = Math.Clamp(take, 1, MaxTake);

        // Grab the most recent {take} messages, then flip to ascending (oldest first).
        var messages = await db.Messages
            .Include(m => m.User)
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .Select(m => new MessageDto
            {
                Username = m.User.UserName ?? string.Empty,
                Content = m.Content,
                SentAt = m.SentAt
            })
            .ToListAsync();

        messages.Reverse();

        return Results.Ok(messages);
    }
}
