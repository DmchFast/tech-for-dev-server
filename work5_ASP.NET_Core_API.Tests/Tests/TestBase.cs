using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using work5_ASP.NET_Core_API.Services;
using Xunit;

namespace work5_ASP.NET_Core_API.Tests;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly Uri BaseAddress;
    private readonly IServiceScope _scope;
    protected readonly ITaskStorage TaskStorage;

    public TestBase(WebApplicationFactory<Program> factory)
    {
        var appFactory = factory.WithWebHostBuilder(builder =>
        {
            // Можно переопределить сервисы, если нужно
        });
        Client = appFactory.CreateClient();
        BaseAddress = Client.BaseAddress ?? new Uri("http://localhost");
        _scope = appFactory.Services.CreateScope();
        TaskStorage = _scope.ServiceProvider.GetRequiredService<ITaskStorage>();
        TaskStorage.Clear();
    }

    public void Dispose()
    {
        TaskStorage.Clear();
        _scope?.Dispose();
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}