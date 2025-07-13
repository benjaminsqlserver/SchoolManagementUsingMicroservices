namespace UserManagement.Domain.Entities;

public class UserDetails
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty; // Username
    public string Password { get; set; } = string.Empty; // Encrypted
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserInRole> UserInRoles { get; set; } = new List<UserInRole>();
}