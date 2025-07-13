using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly UserManagementDbContext _context;

    public RoleService(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        role.RoleID = Guid.NewGuid();
        role.CreatedAt = DateTime.UtcNow;
        role.UpdatedAt = DateTime.UtcNow;

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId)
    {
        return await _context.Roles.FindAsync(roleId);
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task UpdateRoleAsync(Role role)
    {
        role.UpdatedAt = DateTime.UtcNow;
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        var role = await _context.Roles.FindAsync(roleId);
        if (role != null)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
    }
}