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
}
