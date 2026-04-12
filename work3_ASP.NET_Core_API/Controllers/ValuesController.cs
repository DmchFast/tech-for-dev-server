using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using work3_ASP.NET_Core_API.Models;
using work3_ASP.NET_Core_API.Services;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Security.Cryptography;

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

        var hashed = BCryptNet.HashPassword(dto.Password);
        _userRepo.TryAdd(dto.Username, hashed);
        return Ok(new { message = "New user created" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginDto dto)
    {
        if (!_userRepo.TryGetTimingSafe(dto.Username, out var hashed))
            return NotFound(new { detail = "User not found" });

        bool valid = BCryptNet.Verify(dto.Password, hashed);
        if (!valid)
            return Unauthorized(new { detail = "Authorization failed" });

        var token = GenerateJwtToken(dto.Username);
        return Ok(new { access_token = token, token_type = "bearer" });
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = double.Parse(jwtSettings["ExpiryMinutes"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username)
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

    [Authorize]
    [HttpGet("resource")]
    public IActionResult GetProtectedResource()
    {
        var username = User.Identity?.Name;
        return Ok(new { message = $"Access granted to {username}" });
    }
}