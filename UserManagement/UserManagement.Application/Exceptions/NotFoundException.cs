namespace UserManagement.Application.Exceptions;

public class NotFoundException : BaseException
{
    public override int StatusCode => 404;
    public override string ErrorType => "NotFound";

    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entityName, object id) : base($"{entityName} with ID '{id}' was not found.") { }
}
