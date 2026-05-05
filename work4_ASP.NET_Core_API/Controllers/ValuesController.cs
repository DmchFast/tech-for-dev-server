using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using work4_ASP.NET_Core_API.Exceptions;

namespace work4_ASP.NET_Core_API.Controllers;

[Route("api/[controller]")]
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
}
