using System.ComponentModel.DataAnnotations;

namespace work2_ASP.NET_Core_API.Models;

public class UserCreate
{
    [Required(ErrorMessage = "Имя обязательно")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Возраст должен быть положительным числом")]
    public int? Age { get; set; }

    public bool IsSubscribed { get; set; }
}
