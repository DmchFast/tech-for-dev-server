using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using work3_ASP.NET_Core_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();            // Swagger

builder.Services.AddSingleton<UserMemoryRepository>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddAuthorization();


var app = builder.Build();

// Режим из конфигурации (appsettings.json)
var mode = builder.Configuration["Mode"];  // "DEV" или "PROD"

if (mode == "DEV")
{
    // Защита Swagger базовой аутентификацией
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic";
                context.Response.StatusCode = 401;
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var encoded = authHeader.Substring("Basic ".Length).Trim();
            string decoded;
            try
            {
                decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            }
            catch
            {
                context.Response.StatusCode = 401;
                return;
            }

            var parts = decoded.Split(':', 2);
            if (parts.Length != 2)
            {
                context.Response.StatusCode = 401;
                return;
            }

            var username = parts[0];
            var password = parts[1];
            var expectedUser = builder.Configuration["DocsAuth:User"] ?? "";
            var expectedPass = builder.Configuration["DocsAuth:Password"] ?? "";

            bool userMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(username),
                Encoding.UTF8.GetBytes(expectedUser));
            bool passMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(expectedPass));

            if (!userMatch || !passMatch)
            {
                context.Response.StatusCode = 401;
                return;
            }

            await next();
        }
        else
        {
            await next();
        }
    });

    app.MapOpenApi();                          // OpenAPI (Scalar) тоже под защитой

    app.UseSwagger();                         // Swagger JSON

    app.UseSwaggerUI();                        // Swagger UI (/swagger)
}
else if (mode == "PROD")
{
    // Полностью скрытие документации (и Swagger, и OpenAPI)
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/openapi"))
        {
            context.Response.StatusCode = 404;
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
