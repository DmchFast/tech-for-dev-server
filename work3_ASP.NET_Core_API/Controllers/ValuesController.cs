using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using work3_ASP.NET_Core_API.Models;
using work3_ASP.NET_Core_API.Services;
using BCryptNet = BCrypt.Net.BCrypt;

namespace work3_ASP.NET_Core_API.Controllers;

[Route("/")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly UserMemoryRepository _userRepo;
    private readonly IConfiguration _config;

    public ValuesController(UserMemoryRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegisterDto dto)
    {
        if (_userRepo.Exists(dto.Username))
            return Conflict(new { detail = "User already exists" });

        // Разрешение только guest или user (admin нельзя зарегистрировать через API)
        string role = (dto.Role == "user") ? "user" : "guest";

        var hashed = BCryptNet.HashPassword(dto.Password);
        _userRepo.TryAdd(dto.Username, hashed, role);

        return StatusCode(201, new { message = "New user created", role = role });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginDto dto)
    {
        if (!_userRepo.TryGetTimingSafe(dto.Username, out var hashed, out var role))
            return NotFound(new { detail = "User not found" });

        bool valid = BCryptNet.Verify(dto.Password, hashed);
        if (!valid)
            return Unauthorized(new { detail = "Authorization failed" });

        var token = GenerateJwtToken(dto.Username, role!);
        return Ok(new { access_token = token, token_type = "bearer" });
    }

    // Basic Auth
    [HttpGet("login-basic")]
    public IActionResult LoginBasic()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Access to secret\"");
            return Unauthorized(new { detail = "Basic authentication required" });
        }

        try
        {
            var encoded = authHeader.Substring("Basic ".Length).Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encoded)).Split(':');
            if (credentials.Length != 2)
                return BadRequest(new { detail = "Invalid Authorization header format" });

            var username = credentials[0];
            var password = credentials[1];

            // Тайминг-безопасный поиск (защита от атак по времени)
            if (!_userRepo.TryGetTimingSafe(username, out var hashed, out _))
            {
                Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Access to secret\"");
                return Unauthorized(new { detail = "Invalid credentials" });
            }

            if (!BCryptNet.Verify(password, hashed))
            {
                Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Access to secret\"");
                return Unauthorized(new { detail = "Invalid credentials" });
            }

            return Ok("You got my secret, welcome");
        }
        catch
        {
            return BadRequest(new { detail = "Error decoding authentication header" });
        }
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = double.Parse(jwtSettings["ExpiryMinutes"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(secretKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Защищённый ресурс (доступ для admin и user)
    [Authorize(Roles = "admin,user")]
    [HttpGet("resource")]
    public IActionResult GetProtectedResource()
    {
        var username = User.Identity?.Name;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return Ok(new { message = $"Access granted to {username} with role {role}" });
    }

    // Admin (только для admin)
    [Authorize(Roles = "admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "Welcome, admin! You have full access." });
    }
}