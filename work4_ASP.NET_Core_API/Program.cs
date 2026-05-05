using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using work4_ASP.NET_Core_API.Data;
using work4_ASP.NET_Core_API.Middleware;
using work4_ASP.NET_Core_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();            // Swagger

// Настройка валидации
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                k => k.Key,
                v => v.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
            );

        var problem = new
        {
            StatusCode = 400,
            Message = "Validation failed",
            Errors = errors
        };
        return new BadRequestObjectResult(problem);
    };
});


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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
