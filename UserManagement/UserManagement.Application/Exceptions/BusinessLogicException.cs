// UserManagement/UserManagement.Application/Exceptions/BusinessLogicException.cs
namespace UserManagement.Application.Exceptions;

public class BusinessLogicException : BaseException
{
    public override int StatusCode => 422;
    public override string ErrorType => "BusinessLogicError";

    public BusinessLogicException(string message) : base(message) { }
}
