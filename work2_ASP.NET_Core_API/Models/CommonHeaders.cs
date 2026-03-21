using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace work2_ASP.NET_Core_API.Models;

public class CommonHeaders
{
    [FromHeader(Name = "User-Agent")]
    [Required(ErrorMessage = "User-Agent обязателен")]
    public string UserAgent { get; set; } = string.Empty;

    [FromHeader(Name = "Accept-Language")]
    [Required(ErrorMessage = "Accept-Language обязателен")]
    [AcceptLanguageValidation]
    public string AcceptLanguage { get; set; } = string.Empty;
}

public class AcceptLanguageValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string lang && !string.IsNullOrWhiteSpace(lang))
        {
            // Простейшая проверка формата Accept-Language
            if (!Regex.IsMatch(lang, @"^[a-z]{2}(-[A-Z]{2})?(;q=[0-9.]*)?(,|$)"))
            {
                return new ValidationResult("Неверный формат Accept-Language.");
            }
            return ValidationResult.Success;
        }
        return new ValidationResult("Accept-Language не может быть пустым.");
    }
}