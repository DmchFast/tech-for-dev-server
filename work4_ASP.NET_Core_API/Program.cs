using Microsoft.EntityFrameworkCore;
using work4_ASP.NET_Core_API.Data;
using work4_ASP.NET_Core_API.Models;

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();   // миграция при запуске приложения

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Title = "Product1", Price = 100, Count = 10 },
            new Product { Title = "Product2", Price = 200, Count = 5 }
        );
        db.SaveChanges();
    }
}

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
