namespace SchoolClass.Application.DTOs;

public class SchoolClassResponseDto
{
    public Guid SchoolClassID { get; set; }
    public string SchoolClassName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}