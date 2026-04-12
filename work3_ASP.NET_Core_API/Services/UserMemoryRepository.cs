using System.Collections.Concurrent;

namespace work3_ASP.NET_Core_API.Services;

public class UserMemoryRepository
{
    private readonly ConcurrentDictionary<string, string> _users = new(); // username -> hashedPassword

    public bool TryAdd(string username, string hashedPassword) => _users.TryAdd(username, hashedPassword);
    public bool TryGet(string username, out string? hashedPassword) => _users.TryGetValue(username, out hashedPassword);
    public bool Exists(string username) => _users.ContainsKey(username);
}