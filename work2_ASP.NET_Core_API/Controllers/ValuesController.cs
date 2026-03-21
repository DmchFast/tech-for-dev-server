using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
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


    // Секретный ключ для подписи (из конфигурации)
    private string SecretKey => _configuration["SecretKey"] ?? throw new InvalidOperationException("SecretKey not configured");

    private readonly IConfiguration _configuration;

    public ValuesController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string ComputeSignature(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        // Преобразование в base64url
        return Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").TrimEnd('=');
    }

    private bool VerifySignature(string data, string signature)
    {
        var computed = ComputeSignature(data);
        return computed == signature;
    }

    [HttpPost("login_signed")]
    public IActionResult LoginSigned([FromBody] LoginRequest request)
    {
        if (_validUsers.TryGetValue(request.Username, out var validPassword)
            && validPassword == request.Password)
        {
            var userId = Guid.NewGuid().ToString(); // уникальный идентификатор пользователя
            var signature = ComputeSignature(userId);
            var sessionToken = $"{userId}.{signature}";

            Response.Cookies.Append("session_token", sessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            return Ok(new { message = "Успешный вход (с подписью)" });
        }

        return Unauthorized(new { message = "Неверные учетные данные" });
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        if (!Request.Cookies.TryGetValue("session_token", out var token))
        {
            return Unauthorized(new { message = "Неавторизован" });
        }

        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            return Unauthorized(new { message = "Недействительный сеанс" });
        }

        var userId = parts[0];
        var signature = parts[1];

        if (!VerifySignature(userId, signature))
        {
            return Unauthorized(new { message = "Недействительный сеанс" });
        }

        return Ok(new
        {
            user_id = userId,
            username = "user",
            email = "user@example.com"
        });
    }

    private string CreateSessionToken(string userId, long timestamp)
    {
        var data = $"{userId}.{timestamp}";
        var signature = ComputeSignature(data);
        return $"{data}.{signature}";
    }

    private (string userId, long timestamp, bool valid) ValidateSessionToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return (null!, 0, false);

        var userId = parts[0];
        var timestampStr = parts[1];
        var signature = parts[2];

        if (!long.TryParse(timestampStr, out var timestamp))
            return (null!, 0, false);

        var data = $"{userId}.{timestamp}";
        if (!VerifySignature(data, signature))
            return (null!, 0, false);

        return (userId, timestamp, true);
    }

    [HttpPost("login_extended")]
    public IActionResult LoginExtended([FromBody] LoginRequest request)
    {
        if (_validUsers.TryGetValue(request.Username, out var validPassword)
            && validPassword == request.Password)
        {
            var userId = Guid.NewGuid().ToString();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var sessionToken = CreateSessionToken(userId, timestamp);

            Response.Cookies.Append("session_token", sessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                MaxAge = TimeSpan.FromMinutes(5)
            });

            return Ok(new { message = "Успешный вход (с продлением)" });
        }

        return Unauthorized(new { message = "Неверные учетные данные" });
    }

    [HttpGet("profile_extended")]
    public IActionResult GetProfileExtended()
    {
        if (!Request.Cookies.TryGetValue("session_token", out var token))
        {
            return Unauthorized(new { message = "Неавторизован" });
        }

        var (userId, timestamp, valid) = ValidateSessionToken(token);
        if (!valid)
        {
            return Unauthorized(new { message = "Недействительный сеанс" });
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var diffSeconds = now - timestamp;

        // Сессия истекла (5 минут)
        if (diffSeconds > 300)
        {
            Response.Cookies.Delete("session_token");
            return Unauthorized(new { message = "Сеанс истек" });
        }

        // Обновление
        bool shouldRefresh = diffSeconds >= 180 && diffSeconds <= 300;

        if (shouldRefresh)
        {
            var newToken = CreateSessionToken(userId, now);
            Response.Cookies.Append("session_token", newToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                MaxAge = TimeSpan.FromMinutes(5)
            });
        }

        return Ok(new
        {
            user_id = userId,
            username = "user",
            email = "user@example.com"
        });
    }


    [HttpGet("headers")]
    public IActionResult GetHeaders([FromHeader] CommonHeaders headers)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(new
        {
            User_Agent = headers.UserAgent,
            Accept_Language = headers.AcceptLanguage
        });
    }

    [HttpGet("info")]
    public IActionResult GetInfo([FromHeader] CommonHeaders headers)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Response.Headers.Append("X-Server-Time", DateTime.UtcNow.ToString("o"));

        return Ok(new
        {
            message = "Заголовки успешно обработаны.",
            headers = new
            {
                User_Agent = headers.UserAgent,
                Accept_Language = headers.AcceptLanguage
            }
        });
    }
}