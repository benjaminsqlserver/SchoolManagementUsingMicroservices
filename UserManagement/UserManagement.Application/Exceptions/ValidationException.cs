namespace UserManagement.Application.Exceptions;

public class ValidationException : BaseException
{
    public override int StatusCode => 400;
    public override string ErrorType => "ValidationError";
    public List<ValidationErrorDto> ValidationErrors { get; set; } = new();

    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, List<ValidationErrorDto> validationErrors) : base(message)
    {
        ValidationErrors = validationErrors;
    }
}