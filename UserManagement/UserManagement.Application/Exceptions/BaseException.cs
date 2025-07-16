namespace UserManagement.Application.Exceptions;

public abstract class BaseException : Exception
{
    public abstract int StatusCode { get; }
    public abstract string ErrorType { get; }
    public Dictionary<string, object>? Details { get; set; }

    protected BaseException(string message) : base(message) { }
    protected BaseException(string message, Exception innerException) : base(message, innerException) { }
}