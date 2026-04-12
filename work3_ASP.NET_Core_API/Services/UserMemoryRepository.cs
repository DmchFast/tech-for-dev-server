using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace work3_ASP.NET_Core_API.Services;

public class UserMemoryRepository
{
    private readonly ConcurrentDictionary<string, string> _users = new(); // username -> hashedPassword

    public bool TryAdd(string username, string hashedPassword) => _users.TryAdd(username, hashedPassword);
    public bool TryGet(string username, out string? hashedPassword) => _users.TryGetValue(username, out hashedPassword);

    public bool Exists(string username) => _users.ContainsKey(username);

    public bool TryGetTimingSafe(string username, out string? hashedPassword)
    {
        // Перебор всех ключей, сравнивая каждый с искомым именем
        foreach (var kvp in _users)
        {
            bool match = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(kvp.Key),
                Encoding.UTF8.GetBytes(username)
            );
            if (match)
            {
                hashedPassword = kvp.Value;
                return true;
            }
        }
        hashedPassword = null;
        return false;
    }
}