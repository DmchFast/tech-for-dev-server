using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using work2_ASP.NET_Core_API.Models;

namespace work2_ASP.NET_Core_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    [HttpPost("create_user")]
    public IActionResult CreateUser([FromBody] UserCreate user)
    {
        return Ok(user);
    }

    private static readonly List<Product> _products = new()
    {
        new() { ProductId = 123, Name = "Smartphone", Category = "Electronics", Price = 599.99m },
        new() { ProductId = 456, Name = "Phone Case", Category = "Accessories", Price = 19.99m },
        new() { ProductId = 789, Name = "Iphone", Category = "Electronics", Price = 1299.99m },
        new() { ProductId = 101, Name = "Headphones", Category = "Accessories", Price = 99.99m },
        new() { ProductId = 202, Name = "Smartwatch", Category = "Electronics", Price = 299.99m }
    };

    [HttpGet("product/{product_id:int}")]
    public IActionResult GetProduct(int product_id)
    {
        var product = _products.FirstOrDefault(p => p.ProductId == product_id);

        if (product == null)
        {
            return NotFound(new { message = "Продукт не найден" });
        }

        return Ok(product);
    }

    [HttpGet("products/search")]
    public IActionResult SearchProducts(
        [FromQuery] string keyword,
        [FromQuery] string? category,
        [FromQuery] int limit = 10)
    {
        var query = _products.AsQueryable();

        // Фильтрация по слову
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        // Фильтрация по категории
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        var results = query.Take(limit).ToList();
        return Ok(results);
    }

    // Пользователь
    private static readonly Dictionary<string, string> _validUsers = new()
    {
        { "user", "12345" }
    };

    private static readonly Dictionary<string, string> _sessions = new();

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (_validUsers.TryGetValue(request.Username, out var validPassword)
            && validPassword == request.Password)
        {
            // Создание токен
            var sessionToken = Guid.NewGuid().ToString();

            _sessions[sessionToken] = request.Username;

            // cookie
            Response.Cookies.Append("session_token", sessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                MaxAge = TimeSpan.FromMinutes(30)
            });

            return Ok(new { message = "Успешный вход" });
        }

        return Unauthorized(new { message = "Неверные учетные данные" });
    }

    [HttpGet("user")]
    public IActionResult GetUserProfile()
    {
        // Наличие cookie
        if (!Request.Cookies.TryGetValue("session_token", out var sessionToken))
        {
            return Unauthorized(new { message = "Неавторизован" });
        }

        // Валидность токена
        if (!_sessions.TryGetValue(sessionToken, out var username))
        {
            return Unauthorized(new { message = "Неавторизован" });
        }

        return Ok(new
        {
            username = username,
            profile = new
            {
                name = username,
                email = $"{username}@example.com"
            }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        if (Request.Cookies.TryGetValue("session_token", out var sessionToken))
        {
            _sessions.Remove(sessionToken);
        }

        Response.Cookies.Delete("session_token");
        return Ok(new { message = "Выход выполнен" });
    }
}