namespace UserManagement.Domain.Entities;

public class UserInRole
{
    public Guid UserID { get; set; }
    public Guid RoleID { get; set; }
    public DateTime AssignedAt { get; set; }
    
    // Navigation properties
    public virtual UserDetails User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}