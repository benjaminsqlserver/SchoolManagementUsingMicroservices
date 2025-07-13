using UserManagement.Domain.Entities;

namespace UserManagement.Application.Interfaces;

public interface IUserService
{
    Task<UserDetails> CreateUserAsync(UserDetails user, Guid roleId);
    Task<UserDetails?> GetUserByIdAsync(Guid id);
    Task<UserDetails?> GetUserByEmailAsync(string email);
    Task<bool> ValidateUserCredentialsAsync(string email, string password);
    Task<List<UserDetails>> GetAllUsersAsync();
    Task UpdateUserAsync(UserDetails user);
    Task DeleteUserAsync(Guid id);
}