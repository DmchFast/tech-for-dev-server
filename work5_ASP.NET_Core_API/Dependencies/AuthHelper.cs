using Microsoft.AspNetCore.Http;
using work5_ASP.NET_Core_API.Models;

namespace work5_ASP.NET_Core_API.Dependencies;

public static class AuthHelper
{
    public static User? GetCurrentUser(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdStr))
            return null;

        if (!int.TryParse(userIdStr, out var userId))
            return null;

        var role = httpContext.Request.Headers["X-User-Role"].FirstOrDefault() ?? UserRoles.User;
        return new User { Id = userId, Role = role };
    }

    public static bool IsAdmin(User user) => user?.Role == UserRoles.Admin;
}
