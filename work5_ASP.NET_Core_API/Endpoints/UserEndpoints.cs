using Microsoft.AspNetCore.Http;
using work5_ASP.NET_Core_API.Dependencies;
using work5_ASP.NET_Core_API.Models;

namespace work5_ASP.NET_Core_API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .WithTags("users")
            .AddEndpointFilter(async (context, next) =>
            {
                var user = AuthHelper.GetCurrentUser(context.HttpContext);
                if (user == null)
                    return Results.Unauthorized();
                context.HttpContext.Items["User"] = user;
                return await next(context);
            });

        group.MapGet("/me", (HttpContext httpContext) =>
        {
            var user = (User)httpContext.Items["User"]!;
            return Results.Ok(new { user.Id, user.Role });
        });

        group.MapGet("/{id:int}", (int id, HttpContext httpContext) =>
        {
            var currentUser = (User)httpContext.Items["User"]!;
            if (currentUser.Role != UserRoles.Admin && currentUser.Id != id)
                return Results.StatusCode(403); // вместо Forbid()
            return Results.Ok(new { Id = id, Role = "user" });
        });
    }
}