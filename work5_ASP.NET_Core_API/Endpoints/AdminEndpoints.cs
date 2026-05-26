using Microsoft.AspNetCore.Http;
using work5_ASP.NET_Core_API.Dependencies;
using work5_ASP.NET_Core_API.Services;

namespace work5_ASP.NET_Core_API.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin")
            .WithTags("admin")
            .AddEndpointFilter(async (context, next) =>
            {
                var user = AuthHelper.GetCurrentUser(context.HttpContext);
                if (user == null)
                    return Results.Unauthorized();
                if (!AuthHelper.IsAdmin(user))
                    return Results.StatusCode(403); // вместо Forbid()
                context.HttpContext.Items["User"] = user;
                return await next(context);
            });

        group.MapGet("/stats", (ITaskStorage storage) =>
        {
            var tasks = storage.GetAllTasks();
            var stats = new
            {
                total_tasks = tasks.Count(),
                by_status = tasks.GroupBy(t => t.Status)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
            return Results.Ok(stats);
        });

        group.MapDelete("/tasks/{id:int}", async (int id, ITaskStorage storage) =>
        {
            var task = storage.GetTaskById(id);
            if (task == null)
                return Results.NotFound();
            storage.DeleteTask(id);
            return Results.NoContent();
        });
    }
}