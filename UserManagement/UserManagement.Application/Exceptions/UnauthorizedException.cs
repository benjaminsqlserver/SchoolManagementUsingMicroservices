// UserManagement/UserManagement.Application/Exceptions/UnauthorizedException.cs
namespace UserManagement.Application.Exceptions;

public class UnauthorizedException : BaseException
{
    public override int StatusCode => 401;
    public override string ErrorType => "Unauthorized";

    public UnauthorizedException(string message) : base(message) { }
}