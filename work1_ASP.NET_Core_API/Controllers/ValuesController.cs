using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using work1_ASP.NET_Core_API.Models;

namespace work1_ASP.NET_Core_API.Controllers;

[Route("/")]
[ApiController]
public class ValuesController : ControllerBase
{
    [HttpGet]
    public object Start()
    {
        return new { message = "Добро пожаловать в моё приложение ASP.NET Core!" };
    }

    [HttpGet("html")]
    public IActionResult Html()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "page", "index.html");
        return PhysicalFile(filePath, "text/html");
    }

    [HttpPost("calculate")]
    public IActionResult Calculate(int num1, int num2)
    {
        var sum = num1 + num2;
        return Ok(new { result = sum });
    }

    [HttpGet("users")]
    public IActionResult Users()
    {
        var users = new User
        {
            Id = 1,
            Name = "Дмитрий Чуваев"
        };

        return Ok(users);
    }

    [HttpPost("user")]
    public IActionResult User(UserAge user)
    {
        bool adult = user.Age > 18;

        var result = new
        {
            user.Name,
            user.Age,
            is_adult = adult

        };

        return Ok(result);
    }

    private static List<Feedback> _feedbacks = new();

    [HttpPost("feedback")]
    public IActionResult Feedback(Feedback f)
    {
        string[] forbiddenWords = { "крингк", "рофл", "вайб" };

        foreach (var word in forbiddenWords)
        {
            if (f.Mes.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                return UnprocessableEntity(new
                {
                    error = $"Использование недопустимых слов"
                });
            }
        }

        _feedbacks.Add(f);
        return Ok(new { message = $"Feedback received. Thank you, {f.Name}." });
    }

    [HttpGet("feedbacks")]
    public IActionResult Feedbacks()
    {
        return Ok(_feedbacks);
    }

    [HttpDelete("feedbacks")]
    public IActionResult ClearFeedbacks()
    {
        _feedbacks.Clear();
        return Ok(new { message = "Все отзывы удалены" });
    }
}
