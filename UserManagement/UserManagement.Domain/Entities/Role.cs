namespace UserManagement.Domain.Entities;

public class Role
{
    public Guid RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserInRole> UserInRoles { get; set; } = new List<UserInRole>();
}