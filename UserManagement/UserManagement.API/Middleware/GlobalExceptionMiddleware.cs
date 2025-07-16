// UserManagement/UserManagement.API/Middleware/GlobalExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using UserManagement.Application.DTOs;
using UserManagement.Application.Exceptions;

namespace UserManagement.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var errorResponse = CreateErrorResponse(exception, traceId);

        // Log the exception
        LogException(exception, traceId, context);

        // Set response
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private ErrorResponseDto CreateErrorResponse(Exception exception, string traceId)
    {
        return exception switch
        {
            BaseException baseEx => new ErrorResponseDto
            {
                Type = baseEx.ErrorType,
                Message = baseEx.Message,
                StatusCode = baseEx.StatusCode,
                TraceId = traceId,
                Details = baseEx.Details,
                ValidationErrors = baseEx is ValidationException validationEx ? validationEx.ValidationErrors : null
            },

            ArgumentException argEx => new ErrorResponseDto
            {
                Type = "ArgumentError",
                Message = argEx.Message,
                StatusCode = 400,
                TraceId = traceId
            },

            UnauthorizedAccessException => new ErrorResponseDto
            {
                Type = "Unauthorized",
                Message = "Access denied.",
                StatusCode = 401,
                TraceId = traceId
            },

            TimeoutException => new ErrorResponseDto
            {
                Type = "Timeout",
                Message = "The request timed out.",
                StatusCode = 408,
                TraceId = traceId
            },

            _ => new ErrorResponseDto
            {
                Type = "InternalServerError",
                Message = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                StatusCode = 500,
                TraceId = traceId,
                Details = _environment.IsDevelopment() ? new Dictionary<string, object>
                {
                    ["ExceptionType"] = exception.GetType().Name,
                    ["StackTrace"] = exception.StackTrace ?? string.Empty
                } : null
            }
        };
    }

    private void LogException(Exception exception, string traceId, HttpContext context)
    {
        var logLevel = exception switch
        {
            BaseException baseEx when baseEx.StatusCode < 500 => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(logLevel, exception,
            "Exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId, context.Request.Path, context.Request.Method);
    }
}