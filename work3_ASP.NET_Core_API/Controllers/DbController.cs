using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using work3_ASP.NET_Core_API.Data;
using work3_ASP.NET_Core_API.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace work3_ASP.NET_Core_API.Controllers;

[Route("db/")]
[ApiController]
public class DbController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public DbController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register-plain")]
    public async Task<IActionResult> RegisterPlain([FromBody] UserRegisterPlainDto dto)
    {
        if (await _context.PlainUsers.AnyAsync(u => u.Username == dto.Username))
            return Conflict(new { detail = "User already exists" });

        var user = new PlainUser
        {
            Username = dto.Username,
            Password = dto.Password   // открытый пароль (без хеширования)
        };
        _context.PlainUsers.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered with plain password (PostgreSQL)" });
    }

    // Обычная регистрация (с хешированием)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return Conflict(new { detail = "User already exists" });

        var user = new User
        {
            Username = dto.Username,
            HashedPassword = BCryptNet.HashPassword(dto.Password),
            Role = dto.Role == "admin" ? "admin" : (dto.Role == "user" ? "user" : "guest")
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(201, new { message = "User registered successfully!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !BCryptNet.Verify(dto.Password, user.HashedPassword))
            return Unauthorized(new { detail = "Invalid credentials" });

        var token = GenerateJwtToken(user.Username, user.Role);
        return Ok(new TokenResponseDto { AccessToken = token });
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


    [Authorize(Roles = "admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.Select(u => new { u.Id, u.Username, u.Role }).ToListAsync();
        return Ok(users);
    }

    [Authorize(Roles = "guest")]
    [HttpGet("guest-info")]
    public IActionResult GuestInfo() => Ok(new { message = "Guest read-only access" });

    [Authorize(Roles = "user")]
    [HttpGet("user-info")]
    public IActionResult UserInfo() => Ok(new { message = "User can read and update" });

    [Authorize(Roles = "admin")]
    [HttpGet("admin-info")]
    public IActionResult AdminInfo() => Ok(new { message = "Admin full control" });

    // CRUD для Todo с ролями
    [Authorize(Roles = "admin,user")]
    [HttpPost("todos")]
    public async Task<IActionResult> CreateTodo([FromBody] TodoCreateDto dto)
    {
        var todo = new Todo
        {
            Title = dto.Title,
            Description = dto.Description,
            Completed = false
        };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    [Authorize] // все авторизованные (guest, user, admin с чтением)
    [HttpGet("todos/{id}")]
    public async Task<IActionResult> GetTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null) return NotFound();
        return Ok(todo);
    }

    [Authorize(Roles = "admin,user")]
    [HttpPut("todos/{id}")]
    public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto dto)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null) return NotFound();

        todo.Title = dto.Title;
        todo.Description = dto.Description;
        todo.Completed = dto.Completed;

        await _context.SaveChangesAsync();
        return Ok(todo);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("todos/{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null) return NotFound();

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Todo deleted" });
    }
}