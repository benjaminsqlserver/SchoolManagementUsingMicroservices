namespace SchoolClass.Domain.Entities;

public class SchoolClasse
{
    public Guid SchoolClassID { get; set; }
    public string SchoolClassName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}