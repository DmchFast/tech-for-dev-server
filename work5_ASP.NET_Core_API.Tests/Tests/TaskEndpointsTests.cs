using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using work5_ASP.NET_Core_API.Endpoints;
using work5_ASP.NET_Core_API.Models;
using Xunit;

namespace work5_ASP.NET_Core_API.Tests;

public class TaskEndpointsTests : TestBase
{
    public TaskEndpointsTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task CreateTask_Success_Returns201()
    {
        Client.DefaultRequestHeaders.Add("X-User-Id", "10");
        var request = new TaskEndpoints.TaskCreateRequest("Test title", "desc", "todo", 3);
        var response = await Client.PostAsJsonAsync("/tasks", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskItem>();
        Assert.NotNull(task);
        Assert.Equal(10, task.OwnerId);
    }

    [Fact]
    public async Task CreateTask_InvalidTitle_Returns422()
    {
        Client.DefaultRequestHeaders.Add("X-User-Id", "10");
        var request = new TaskEndpoints.TaskCreateRequest("ab", null, "todo", 3);
        var response = await Client.PostAsJsonAsync("/tasks", request);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_NoUserId_Returns401()
    {
        var request = new TaskEndpoints.TaskCreateRequest("Title", null, "todo", 3);
        var response = await Client.PostAsJsonAsync("/tasks", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTasks_OnlyOwnTasks_ReturnsFiltered()
    {
        Client.DefaultRequestHeaders.Add("X-User-Id", "10");
        await CreateTestTask("Task1", 10);
        await CreateTestTask("Task2", 10);
        await CreateTestTask("Task3", 20);

        var response = await Client.GetAsync("/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
        Assert.Equal(2, tasks?.Count);
        Assert.All(tasks!, t => Assert.Equal(10, t.OwnerId));
    }

    [Fact]
    public async Task GetTasks_FilterByStatusAndPriority_Works()
    {
        Client.DefaultRequestHeaders.Add("X-User-Id", "10");
        await CreateTestTask("Todo low", 10, status: "todo", priority: 1);
        await CreateTestTask("Todo high", 10, status: "todo", priority: 5);
        await CreateTestTask("Done", 10, status: "done", priority: 3);

        var response = await Client.GetAsync("/tasks?status=todo&minPriority=3");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
        Assert.Single(tasks!);
        Assert.Equal("Todo high", tasks![0].Title);
    }

    [Fact]
    public async Task PatchStatus_Success_ReturnsOk()
    {
        Client.DefaultRequestHeaders.Add("X-User-Id", "10");
        var task = await CreateTestTask("Test", 10);
        var update = new TaskEndpoints.StatusUpdateRequest("done");
        var response = await Client.PatchAsJsonAsync($"/tasks/{task.Id}/status", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TaskItem>();
        Assert.Equal("done", updated?.Status);
    }

    [Fact]
    public async Task GetTask_NotFoundOrForeign_Returns404()
    {
        Client.DefaultRequestHeaders.Add("X-User-Id", "10");
        var foreign = await CreateTestTask("Foreign", 20);
        var response = await Client.GetAsync($"/tasks/{foreign.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTask_Success_Returns204()
    {
        Client.DefaultRequestHeaders.Add("X-User-Id", "10");
        var task = await CreateTestTask("ToDelete", 10);
        var response = await Client.DeleteAsync($"/tasks/{task.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await Client.GetAsync($"/tasks/{task.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    // Теперь используем существующий Client с временной заменой заголовков
    private async Task<TaskItem> CreateTestTask(string title, int ownerId, string status = "todo", int priority = 1)
    {
        // Сохраняем старые заголовки
        var oldHeaders = Client.DefaultRequestHeaders.ToDictionary(h => h.Key, h => h.Value.FirstOrDefault());
        // Устанавливаем нужный X-User-Id
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        Client.DefaultRequestHeaders.Add("X-User-Id", ownerId.ToString());

        var request = new TaskEndpoints.TaskCreateRequest(title, null, status, priority);
        var response = await Client.PostAsJsonAsync("/tasks", request);
        var task = await response.Content.ReadFromJsonAsync<TaskItem>();

        // Восстанавливаем заголовки
        Client.DefaultRequestHeaders.Remove("X-User-Id");
        foreach (var kv in oldHeaders)
        {
            if (kv.Value != null)
                Client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
        }

        return task ?? throw new InvalidOperationException("Failed to create test task");
    }
}