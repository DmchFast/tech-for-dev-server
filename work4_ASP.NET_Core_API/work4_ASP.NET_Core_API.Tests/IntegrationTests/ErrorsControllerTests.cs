using System.Net;
using System.Net.Http.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using work4_ASP.NET_Core_API.Controllers; // связь с  Errors, UserInput и UserOutput

namespace work4_ASP.NET_Core_API.work4_ASP.NET_Core_API.Tests.IntegrationTests;

public class ErrorsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ErrorsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task ClearDb()
    {
        await _client.DeleteAsync("/api/users/reset");
    }

    private static readonly Faker<ErrorsController.UserInput> UserFaker =
        new Faker<ErrorsController.UserInput>()
            .CustomInstantiator(f => new ErrorsController.UserInput(f.Internet.UserName(), f.Random.Int(18, 99)));

    [Fact]
    public async Task CreateUser_Returns201_And_ValidResponse()
    {
        await ClearDb();
        var input = UserFaker.Generate();

        var response = await _client.PostAsJsonAsync("/api/users", input);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<ErrorsController.UserOutput>();
        Assert.NotNull(user);
        Assert.Equal(input.Username, user!.Username);
        Assert.Equal(input.Age, user.Age);
        Assert.True(user.Id >= 0);
    }

    [Fact]
    public async Task GetUser_Existing_Returns200()
    {
        await ClearDb();
        var input = UserFaker.Generate();
        var createdResp = await _client.PostAsJsonAsync("/api/users", input);
        var created = await createdResp.Content.ReadFromJsonAsync<ErrorsController.UserOutput>();

        var response = await _client.GetAsync($"/api/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<ErrorsController.UserOutput>();
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(input.Username, fetched.Username);
    }

    [Fact]
    public async Task GetUser_NonExisting_Returns404()
    {
        await ClearDb();
        var response = await _client.GetAsync("/api/users/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_Existing_Returns204()
    {
        await ClearDb();
        var input = UserFaker.Generate();
        var createdResp = await _client.PostAsJsonAsync("/api/users", input);
        var created = await createdResp.Content.ReadFromJsonAsync<ErrorsController.UserOutput>();

        var response = await _client.DeleteAsync($"/api/users/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_NonExisting_Returns404()
    {
        await ClearDb();
        var response = await _client.DeleteAsync("/api/users/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}