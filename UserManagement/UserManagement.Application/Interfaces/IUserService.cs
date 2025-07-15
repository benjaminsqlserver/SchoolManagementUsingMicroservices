

// ================================
// 2. Updated Service Interface
// ================================

// UserManagement/UserManagement.Application/Interfaces/IUserService.cs
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IUserService
{
    Task<UserDetails> CreateUserAsync(UserDetails user, Guid roleId);
    Task<UserDetails?> GetUserByIdAsync(Guid id);
    Task<UserDetails?> GetUserByEmailAsync(string email);
    Task<bool> ValidateUserCredentialsAsync(string email, string password);
    Task<PagedResultDto<UserSummaryDto>> GetUsersAsync(UserQueryDto queryDto);
    Task<List<UserDetails>> GetAllUsersAsync(); // Keep for backward compatibility
    Task UpdateUserAsync(UserDetails user);
    Task DeleteUserAsync(Guid id);
}