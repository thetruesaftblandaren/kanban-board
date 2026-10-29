using System.Collections.Concurrent;

namespace Kanban.Infrastructure.Realtime;

public class ConnectionTracker
{
    private readonly ConcurrentDictionary<string, Guid> _connections = new();

    public void Add(string connectionId, Guid userId)
    {
        _connections[connectionId] = userId;
    }

    public void Remove(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public IReadOnlyList<string> GetConnections(Guid userId)
    {
        return _connections
            .Where(c => c.Value == userId)
            .Select(c => c.Key)
            .ToList();
    }
}
