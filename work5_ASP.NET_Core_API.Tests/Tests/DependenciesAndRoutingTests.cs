using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using work5_ASP.NET_Core_API.Endpoints;
using work5_ASP.NET_Core_API.Models;
using work5_ASP.NET_Core_API.Services;
using Xunit;

namespace work5_ASP.NET_Core_API.Tests;

public class DependenciesAndRoutingTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly ITaskStorage _taskStorage;

    public DependenciesAndRoutingTests(WebApplicationFactory<Program> factory)
    {
        var appFactory = factory.WithWebHostBuilder(builder => { });
        _client = appFactory.CreateClient();
        _taskStorage = appFactory.Services.GetRequiredService<ITaskStorage>();
        _taskStorage.Clear();
    }

    public void Dispose()
    {
        _taskStorage.Clear();
        _client?.Dispose();
    }

    [Fact]
    public async Task UsersMe_ReturnsCurrentUser()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "42");
        _client.DefaultRequestHeaders.Add("X-User-Role", "admin");

        var response = await _client.GetAsync("/users/me");
        var userJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = userJson.GetProperty("id").GetInt32();
        var role = userJson.GetProperty("role").GetString();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(42, id);
        Assert.Equal("admin", role);
    }

    [Fact]
    public async Task MissingUserId_Returns401()
    {
        var response = await _client.GetAsync("/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegularUser_CannotAccessAdminStats_Returns403()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "1");
        _client.DefaultRequestHeaders.Add("X-User-Role", "user");

        var response = await _client.GetAsync("/admin/stats");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanAccessStats()
    {
        _client.DefaultRequestHeaders.Add("X-User-Id", "999");
        _client.DefaultRequestHeaders.Add("X-User-Role", "admin");

        var response = await _client.GetAsync("/admin/stats");
        var stats = await response.Content.ReadFromJsonAsync<StatsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(stats);
    }

    [Fact]
    public async Task RegularUser_CannotDeleteForeignTask_ReturnsNotFound()
    {
        var foreignTask = await CreateTaskAsUser(100, "Foreign task");

        _client.DefaultRequestHeaders.Add("X-User-Id", "1");
        _client.DefaultRequestHeaders.Add("X-User-Role", "user");

        var response = await _client.DeleteAsync($"/tasks/{foreignTask.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_CanDeleteAnyTaskViaAdminEndpoint()
    {
        var task = await CreateTaskAsUser(100, "Task to delete by admin");

        _client.DefaultRequestHeaders.Add("X-User-Id", "999");
        _client.DefaultRequestHeaders.Add("X-User-Role", "admin");

        var deleteResponse = await _client.DeleteAsync($"/admin/tasks/{task.Id}");
        var getResponse = await _client.GetAsync($"/tasks/{task.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<TaskItem> CreateTaskAsUser(int ownerId, string title)
    {
        // Сохраняем старые заголовки
        var oldHeaders = _client.DefaultRequestHeaders.ToDictionary(h => h.Key, h => h.Value.FirstOrDefault());
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", ownerId.ToString());

        var request = new TaskEndpoints.TaskCreateRequest(title, null, "todo", 1);
        var response = await _client.PostAsJsonAsync("/tasks", request);
        var task = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Восстанавливаем заголовки
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        foreach (var kv in oldHeaders)
        {
            if (kv.Value != null)
                _client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
        }

        return task ?? throw new InvalidOperationException("Failed to create task");
    }

    private record StatsResponse(int total_tasks, Dictionary<string, int> by_status);
}