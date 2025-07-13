using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IRoleService
{
    Task<Role> CreateRoleAsync(Role role);
    Task<Role?> GetRoleByIdAsync(Guid roleId);
    Task<List<Role>> GetAllRolesAsync();
    Task UpdateRoleAsync(Role role);
    Task DeleteRoleAsync(Guid roleId);
}