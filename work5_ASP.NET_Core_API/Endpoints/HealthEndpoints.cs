namespace work5_ASP.NET_Core_API.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
           .WithTags("health");
    }
}