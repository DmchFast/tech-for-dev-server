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

    private static readonly List<Product> _products = new()
    {
        new() { ProductId = 123, Name = "Smartphone", Category = "Electronics", Price = 599.99m },
        new() { ProductId = 456, Name = "Phone Case", Category = "Accessories", Price = 19.99m },
        new() { ProductId = 789, Name = "Iphone", Category = "Electronics", Price = 1299.99m },
        new() { ProductId = 101, Name = "Headphones", Category = "Accessories", Price = 99.99m },
        new() { ProductId = 202, Name = "Smartwatch", Category = "Electronics", Price = 299.99m }
    };

    [HttpGet("product/{product_id:int}")]
    public IActionResult GetProduct(int product_id)
    {
        var product = _products.FirstOrDefault(p => p.ProductId == product_id);

        if (product == null)
        {
            return NotFound(new { message = "Продукт не найден" });
        }

        return Ok(product);
    }

    [HttpGet("products/search")]
    public IActionResult SearchProducts(
        [FromQuery] string keyword,
        [FromQuery] string? category,
        [FromQuery] int limit = 10)
    {
        var query = _products.AsQueryable();

        // Фильтрация по слову
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        // Фильтрация по категории
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        var results = query.Take(limit).ToList();
        return Ok(results);
    }
}