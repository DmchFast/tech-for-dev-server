using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using work4_ASP.NET_Core_API.Exceptions;
using work4_ASP.NET_Core_API.Models;

namespace work4_ASP.NET_Core_API.Controllers;

[Route("api")]
[ApiController]
public class ErrorsController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, UserData> Db = new();
    private static int _nextId = 1;
    public record UserData(int Id, string Username, int Age);
    public record UserInput(string Username, int Age);
    public record UserOutput(int Id, string Username, int Age);

    [HttpGet("errors/condition")]
    public IActionResult TriggerA()
    {
        throw new CustomExceptionA("Condition not met");
    }

    [HttpGet("errors/resource/{id}")]
    public IActionResult TriggerB(int id)
    {
        if (id <= 0)
            throw new CustomExceptionB($"Resource with id={id} not found");
        return Ok(new { id });
    }

    [HttpPost("validation/user")]
    public IActionResult ValidateUser([FromBody] UserInput user)
    {
        // Пройдена валидация
        return Ok(new { message = "User valid", user.Username });
    }
    // CRUD пользователей in-memory
    [HttpPost("users")]
    public IActionResult CreateUser([FromBody] UserInput input)
    {
        var id = Interlocked.Increment(ref _nextId) - 1;
        var user = new UserData(id, input.Username, input.Age);
        Db[id] = user;
        return CreatedAtAction(nameof(GetUser), new { userId = id },
            new UserOutput(id, user.Username, user.Age));
    }

    [HttpGet("users/{userId}")]
    public IActionResult GetUser(int userId)
    {
        if (!Db.TryGetValue(userId, out var user))
            return NotFound(new { detail = "User not found" });
        return Ok(new UserOutput(user.Id, user.Username, user.Age));
    }

    [HttpDelete("users/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        if (!Db.TryRemove(userId, out _))
            return NotFound(new { detail = "User not found" });
        return NoContent();
    }

    [HttpDelete("users/reset")]
    public IActionResult ResetDb()
    {
        Db.Clear();
        _nextId = 1;
        return Ok();
    }
}
