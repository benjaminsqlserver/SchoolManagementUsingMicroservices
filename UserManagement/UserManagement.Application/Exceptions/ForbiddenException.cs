// UserManagement/UserManagement.Application/Exceptions/ForbiddenException.cs
namespace UserManagement.Application.Exceptions;

public class ForbiddenException : BaseException
{
    public override int StatusCode => 403;
    public override string ErrorType => "Forbidden";

    public ForbiddenException(string message) : base(message) { }
}