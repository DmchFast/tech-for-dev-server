using Microsoft.AspNetCore.Mvc;
using work3_ASP.NET_Core_API.Models;
using work3_ASP.NET_Core_API.Services;
using BCryptNet = BCrypt.Net.BCrypt;

namespace work3_ASP.NET_Core_API.Controllers;

[Route("/")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly UserMemoryRepository _userRepo;

    public ValuesController(UserMemoryRepository userRepo)
    {
        _userRepo = userRepo;
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
        if (!_userRepo.TryGet(dto.Username, out var hashed))
            return NotFound(new { detail = "User not found" });

        bool valid = BCryptNet.Verify(dto.Password, hashed);
        if (!valid)
            return Unauthorized(new { detail = "Authorization failed" });

        return Ok(new { message = $"Welcome, {dto.Username}!" });
    }
}