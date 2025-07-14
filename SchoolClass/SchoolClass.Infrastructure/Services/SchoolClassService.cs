using Microsoft.EntityFrameworkCore;
using SchoolClass.Application.Interfaces;
using SchoolClass.Domain.Entities;
using SchoolClass.Infrastructure.Data;

namespace SchoolClass.Infrastructure.Services;

public class SchoolClassService : ISchoolClassService
{
    private readonly SchoolClassDbContext _context;

    public SchoolClassService(SchoolClassDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolClasse> CreateSchoolClassAsync(SchoolClasse schoolClass)
    {
        schoolClass.SchoolClassID = Guid.NewGuid();
        schoolClass.CreatedAt = DateTime.UtcNow;
        schoolClass.UpdatedAt = DateTime.UtcNow;

        _context.SchoolClasses.Add(schoolClass);
        await _context.SaveChangesAsync();
        return schoolClass;
    }

    public async Task<SchoolClasse?> GetSchoolClassByIdAsync(Guid id)
    {
        return await _context.SchoolClasses.FindAsync(id);
    }

    public async Task<List<SchoolClasse>> GetAllSchoolClassesAsync()
    {
        return await _context.SchoolClasses.ToListAsync();
    }

    public async Task UpdateSchoolClassAsync(SchoolClasse schoolClass)
    {
        schoolClass.UpdatedAt = DateTime.UtcNow;
        _context.SchoolClasses.Update(schoolClass);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSchoolClassAsync(Guid id)
    {
        var schoolClass = await _context.SchoolClasses.FindAsync(id);
        if (schoolClass != null)
        {
            _context.SchoolClasses.Remove(schoolClass);
            await _context.SaveChangesAsync();
        }
    }
}