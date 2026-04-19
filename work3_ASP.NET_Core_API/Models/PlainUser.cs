namespace work3_ASP.NET_Core_API.Models;

public class PlainUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // открытый пароль
}
