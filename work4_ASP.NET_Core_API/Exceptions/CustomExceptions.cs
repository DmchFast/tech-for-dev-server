namespace work4_ASP.NET_Core_API.Exceptions;

public class CustomExceptionA : Exception
{
    public int StatusCode { get; } = 400;
    public CustomExceptionA(string message) : base(message) { }
}

public class CustomExceptionB : Exception
{
    public int StatusCode { get; } = 404;
    public CustomExceptionB(string message) : base(message) { }
}