namespace UserManagement.Application.Exceptions;

public class ConflictException : BaseException
{
    public override int StatusCode => 409;
    public override string ErrorType => "Conflict";

    public ConflictException(string message) : base(message) { }
}
