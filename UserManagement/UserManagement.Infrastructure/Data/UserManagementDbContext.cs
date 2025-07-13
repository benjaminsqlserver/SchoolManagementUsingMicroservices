using Microsoft.EntityFrameworkCore;
using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Data;

public class UserManagementDbContext : DbContext
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : base(options)
    {
    }

    public DbSet<UserDetails> UserDetails { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserInRole> UserInRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure UserDetails
        modelBuilder.Entity<UserDetails>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.EmailAddress).IsUnique();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        // Configure Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleID);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.RoleName).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        // Configure UserInRole (Many-to-Many relationship)
        modelBuilder.Entity<UserInRole>(entity =>
        {
            entity.HasKey(e => new { e.UserID, e.RoleID });
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETDATE()");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserInRoles)
                .HasForeignKey(e => e.UserID);
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserInRoles)
                .HasForeignKey(e => e.RoleID);
        });

        base.OnModelCreating(modelBuilder);
    }
}