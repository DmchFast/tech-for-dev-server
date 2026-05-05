using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using work4_ASP.NET_Core_API.Exceptions;
using work4_ASP.NET_Core_API.Models;

namespace work4_ASP.NET_Core_API.Controllers;

[Route("api")]
[ApiController]
public class ErrorsController : ControllerBase
{
    [HttpGet("condition")]
    public IActionResult TriggerA()
    {
        throw new CustomExceptionA("Condition not met");
    }

    [HttpGet("resource/{id}")]
    public IActionResult TriggerB(int id)
    {
        if (id <= 0)
            throw new CustomExceptionB($"Resource with id={id} not found");
        return Ok(new { id });
    }

    [HttpPost("validation/user")]
    public IActionResult CreateUser([FromBody] UserInput user)
    {
        // Пройдена валидация
        return Ok(new { message = "User valid", user.Username });
    }
}
