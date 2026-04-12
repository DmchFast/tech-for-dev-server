using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using work3_ASP.NET_Core_API.Data;
using work3_ASP.NET_Core_API.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace work3_ASP.NET_Core_API.Controllers;


[Route("db/")]
[ApiController]
public class DbController : ControllerBase
{
    private readonly AppDbContext _context;

    public DbController(AppDbContext context)
    {
        _context = context;
    }

    // POST /db/register
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

        return Ok(new { message = "User registered successfully!" });
    }

    // GET /db/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.Select(u => new { u.Id, u.Username, u.Role }).ToListAsync();
        return Ok(users);
    }

    // CRUD для Todo

    // POST /db/todos
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

    // GET /db/todos/{id}
    [HttpGet("todos/{id}")]
    public async Task<IActionResult> GetTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);
        if (todo == null) return NotFound();
        return Ok(todo);
    }

    // PUT /db/todos/{id}
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

    // DELETE /db/todos/{id}
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