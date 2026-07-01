using System.Collections.Concurrent;

namespace Signalio.Server.Services;

// Tracks currently connected users. Keyed by SignalR connectionId so a user with
// multiple connections is handled correctly per connection.
public class OnlineUserService
{
    private readonly ConcurrentDictionary<string, string> _connections = new();

    public void Add(string connectionId, string username) =>
        _connections[connectionId] = username;

    public void Remove(string connectionId) =>
        _connections.TryRemove(connectionId, out _);

    public IReadOnlyCollection<string> GetAll() =>
        _connections.Values.Distinct().ToList();
}
