// UserManagement/UserManagement.Application/DTOs/ErrorResponseDto.cs
using System.Text.Json.Serialization;

namespace UserManagement.Application.DTOs;

public class ErrorResponseDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Details { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ValidationErrorDto>? ValidationErrors { get; set; }
}
