using System.ComponentModel.DataAnnotations;

namespace work4_ASP.NET_Core_API.Models;

public class UserInput
{
    [Required]
    public string Username { get; set; } = null!;

    [AgeGreaterThan(18, ErrorMessage = "Age must be greater than 18")]
    public int Age { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(16, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 16 characters")]
    public string Password { get; set; } = null!;

    public string Phone { get; set; } = "Unknown";  // базовое состояние
}

// Кастомный атрибут "больше чем"
public class AgeGreaterThanAttribute : ValidationAttribute
{
    private readonly int _min;
    public AgeGreaterThanAttribute(int min) => _min = min;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int age && age > _min)
            return ValidationResult.Success;
        return new ValidationResult($"Age must be greater than {_min}");
    }
}