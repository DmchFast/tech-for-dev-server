using Microsoft.AspNetCore.Http;
using work5_ASP.NET_Core_API.Dependencies;
using work5_ASP.NET_Core_API.Models;
using work5_ASP.NET_Core_API.Services;

namespace work5_ASP.NET_Core_API.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tasks")
            .WithTags("tasks")
            .AddEndpointFilter(async (context, next) =>
            {
                var httpContext = context.HttpContext;
                var user = AuthHelper.GetCurrentUser(httpContext);
                if (user == null)
                    return Results.Unauthorized();
                context.HttpContext.Items["User"] = user;
                return await next(context);
            });

        // POST /tasks
        group.MapPost("/", async (TaskCreateRequest request, ITaskStorage storage, HttpContext httpContext) =>
        {
            var user = (User)httpContext.Items["User"]!;

            if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 3 || request.Title.Length > 80)
                return Results.UnprocessableEntity("Title must be between 3 and 80 characters");

            if (request.Status != null && !new[] { "todo", "in_progress", "done" }.Contains(request.Status))
                return Results.BadRequest("Invalid status");

            if (request.Priority < 1 || request.Priority > 5)
                return Results.BadRequest("Priority must be between 1 and 5");

            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status ?? "todo",
                Priority = request.Priority ?? 1,
                OwnerId = user.Id
            };
            var created = storage.CreateTask(task);
            return Results.Created($"/tasks/{created.Id}", created);
        });

        // GET /tasks
        group.MapGet("/", async (ITaskStorage storage, HttpContext httpContext,
            string? status, int? minPriority) =>
        {
            var user = (User)httpContext.Items["User"]!;
            var tasks = storage.GetAllTasks()
                .Where(t => t.OwnerId == user.Id);

            if (!string.IsNullOrEmpty(status))
                tasks = tasks.Where(t => t.Status == status);
            if (minPriority.HasValue)
                tasks = tasks.Where(t => t.Priority >= minPriority.Value);

            return Results.Ok(tasks);
        });

        // GET /tasks/{id}
        group.MapGet("/{id:int}", async (int id, ITaskStorage storage, HttpContext httpContext) =>
        {
            var user = (User)httpContext.Items["User"]!;
            var task = storage.GetTaskById(id);
            if (task == null || task.OwnerId != user.Id)
                return Results.NotFound();
            return Results.Ok(task);
        });

        // PATCH /tasks/{id}/status
        group.MapPatch("/{id:int}/status", async (int id, StatusUpdateRequest request, ITaskStorage storage, HttpContext httpContext) =>
        {
            var user = (User)httpContext.Items["User"]!;
            var task = storage.GetTaskById(id);
            if (task == null || task.OwnerId != user.Id)
                return Results.NotFound();

            if (!new[] { "todo", "in_progress", "done" }.Contains(request.Status))
                return Results.BadRequest("Invalid status");

            task.Status = request.Status;
            storage.UpdateTask(task);
            return Results.Ok(task);
        });

        // DELETE /tasks/{id}
        group.MapDelete("/{id:int}", async (int id, ITaskStorage storage, HttpContext httpContext) =>
        {
            var user = (User)httpContext.Items["User"]!;
            var task = storage.GetTaskById(id);
            if (task == null || task.OwnerId != user.Id)
                return Results.NotFound();

            storage.DeleteTask(id);
            return Results.NoContent();
        });
    }

    public record TaskCreateRequest(string Title, string? Description, string? Status, int? Priority);
    public record StatusUpdateRequest(string Status);
}