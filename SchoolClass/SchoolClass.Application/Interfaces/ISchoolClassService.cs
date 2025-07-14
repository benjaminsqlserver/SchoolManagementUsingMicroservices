using SchoolClass.Domain.Entities;

namespace SchoolClass.Application.Interfaces;

public interface ISchoolClassService
{
    Task<SchoolClasse> CreateSchoolClassAsync(SchoolClasse schoolClass);
    Task<SchoolClasse?> GetSchoolClassByIdAsync(Guid id);
    Task<List<SchoolClasse>> GetAllSchoolClassesAsync();
    Task UpdateSchoolClassAsync(SchoolClasse schoolClass);
    Task DeleteSchoolClassAsync(Guid id);
}