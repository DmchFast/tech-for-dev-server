using System.ComponentModel.DataAnnotations;

namespace work2_ASP.NET_Core_API.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    public string Password { get; set; } = string.Empty;
}
