using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace work3_ASP.NET_Core_API.Services;

public class UserMemoryRepository
{
    // hashedPassword, role
    private readonly ConcurrentDictionary<string, (string HashedPassword, string Role)> _users = new();

    public bool TryAdd(string username, string hashedPassword, string role = "guest")
    {
        return _users.TryAdd(username, (hashedPassword, role));
    }

    public bool TryGet(string username, out string? hashedPassword, out string? role)
    {
        if (_users.TryGetValue(username, out var tuple))
        {
            hashedPassword = tuple.HashedPassword;
            role = tuple.Role;
            return true;
        }
        hashedPassword = null;
        role = null;
        return false;
    }

    public bool Exists(string username) => _users.ContainsKey(username);

    // Тайминг-безопасный поиск (для логина)
    public bool TryGetTimingSafe(string username, out string? hashedPassword, out string? role)
    {
        foreach (var kvp in _users)
        {
            bool match = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(kvp.Key),
                Encoding.UTF8.GetBytes(username)
            );
            if (match)
            {
                hashedPassword = kvp.Value.HashedPassword;
                role = kvp.Value.Role;
                return true;
            }
        }
        hashedPassword = null;
        role = null;
        return false;
    }
}