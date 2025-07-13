using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Data;
using BCrypt.Net;

namespace UserManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManagementDbContext _context;

    public UserService(UserManagementDbContext context)
    {
        _context = context;
    }

    public async Task<UserDetails> CreateUserAsync(UserDetails user, Guid roleId)
    {
        // Hash password
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _context.UserDetails.Add(user);
        await _context.SaveChangesAsync();

        // Assign role
        var userInRole = new UserInRole
        {
            UserID = user.Id,
            RoleID = roleId,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserInRoles.Add(userInRole);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<UserDetails?> GetUserByIdAsync(Guid id)
    {
        return await _context.UserDetails
            .Include(u => u.UserInRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserDetails?> GetUserByEmailAsync(string email)
    {
        return await _context.UserDetails
            .Include(u => u.UserInRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.EmailAddress == email);
    }

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await _context.UserDetails
            .FirstOrDefaultAsync(u => u.EmailAddress == email);

        if (user == null)
            return false;

        return BCrypt.Net.BCrypt.Verify(password, user.Password);
    }

    public async Task<List<UserDetails>> GetAllUsersAsync()
    {
        return await _context.UserDetails
            .Include(u => u.UserInRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task UpdateUserAsync(UserDetails user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.UserDetails.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _context.UserDetails.FindAsync(id);
        if (user != null)
        {
            _context.UserDetails.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}