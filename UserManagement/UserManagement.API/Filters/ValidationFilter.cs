using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserManagement.Application.DTOs;
using UserManagement.Application.Exceptions;

namespace UserManagement.API.Filters;

public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var validationErrors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new ValidationErrorDto
                {
                    Field = x.Key,
                    Message = e.ErrorMessage,
                    AttemptedValue = x.Value.AttemptedValue
                }))
                .ToList();

            throw new ValidationException("One or more validation errors occurred.", validationErrors);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No implementation needed
    }
}