using Microsoft.EntityFrameworkCore;
using SchoolClass.Domain.Entities;

namespace SchoolClass.Infrastructure.Data;

public class SchoolClassDbContext : DbContext
{
    public SchoolClassDbContext(DbContextOptions<SchoolClassDbContext> options) : base(options)
    {
    }

    public DbSet<SchoolClasse> SchoolClasses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchoolClasse>(entity =>
        {
            entity.HasKey(e => e.SchoolClassID);
            entity.Property(e => e.SchoolClassName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.SchoolClassName).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
        });

        base.OnModelCreating(modelBuilder);
    }
}