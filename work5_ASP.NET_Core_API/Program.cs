using work5_ASP.NET_Core_API.Endpoints;
using work5_ASP.NET_Core_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITaskStorage, InMemoryTaskStorage>();
builder.Services.AddSingleton<IRoomManager, RoomManager>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapHealthEndpoints();
app.MapTaskEndpoints();
app.MapUserEndpoints();
app.MapAdminEndpoints();
app.MapWebSocketEndpoints();

app.Run();

public partial class Program { }