using Microsoft.EntityFrameworkCore;
using work4_ASP.NET_Core_API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();            // Swagger

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();                         // Swagger JSON

    app.UseSwaggerUI();                        // Swagger UI (/swagger)
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
