namespace work4_ASP.NET_Core_API.Models;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Count { get; set; }

}
