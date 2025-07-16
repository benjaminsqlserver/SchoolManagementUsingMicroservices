
// ================================
// 3. Updated Service Implementation
// ================================

// UserManagement/UserManagement.Infrastructure/Services/UserService.cs
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using UserManagement.Application.DTOs;
using UserManagement.Application.Exceptions;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManagementDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManagementDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDetails> CreateUserAsync(UserDetails user, Guid roleId)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == user.EmailAddress);

            if (existingUser != null)
            {
                throw new ConflictException($"A user with email '{user.EmailAddress}' already exists.");
            }

            // Check if role exists
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                throw new NotFoundException("Role", roleId);
            }

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

            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex) when (!(ex is BaseException))
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", user.EmailAddress);
            throw;
        }
    }

    public async Task<UserDetails?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _context.UserDetails
                .Include(u => u.UserInRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new NotFoundException("User", id);
            }

            return user;
        }
        catch (Exception ex) when (!(ex is BaseException))
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task<UserDetails?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _context.UserDetails
                .Include(u => u.UserInRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.EmailAddress == email);

            if (user == null)
            {
                throw new NotFoundException($"User with email '{email}' was not found.");
            }

            return user;
        }
        catch (Exception ex) when (!(ex is BaseException))
        {
            _logger.LogError(ex, "Error retrieving user with email: {Email}", email);
            throw;
        }
    }

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        try
        {
            var user = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == email);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            var isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

            if (!isValid)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            return true;
        }
        catch (Exception ex) when (!(ex is BaseException))
        {
            _logger.LogError(ex, "Error validating credentials for email: {Email}", email);
            throw;
        }
    }


    public async Task<PagedResultDto<UserSummaryDto>> GetUsersAsync(UserQueryDto queryDto)
    {
        try
        {
            var query = _context.UserDetails
                .Include(u => u.UserInRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // Apply filters
            query = ApplyFilters(query, queryDto);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, queryDto.SortBy, queryDto.SortDirection);

            // Apply pagination
            var users = await query
                .Skip((queryDto.Page - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .Select(u => new UserSummaryDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName,
                    DateOfBirth = u.DateOfBirth,
                    Gender = u.Gender,
                    EmailAddress = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    RoleName = u.UserInRoles.FirstOrDefault()!.Role.RoleName,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return new PagedResultDto<UserSummaryDto>
            {
                Items = users,
                TotalCount = totalCount,
                Page = queryDto.Page,
                PageSize = queryDto.PageSize
            };
        }
        catch (Exception ex) when (!(ex is BaseException))
        {
            _logger.LogError(ex, "Error retrieving users with query: {@Query}", queryDto);
            throw;
        }
    }


    private IQueryable<UserDetails> ApplyFilters(IQueryable<UserDetails> query, UserQueryDto queryDto)
    {
        // General search term (searches across multiple fields)
        if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
        {
            var searchTerm = queryDto.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.EmailAddress.ToLower().Contains(searchTerm) ||
                u.PhoneNumber.Contains(searchTerm) ||
                u.UserInRoles.Any(ur => ur.Role.RoleName.ToLower().Contains(searchTerm))
            );
        }

        // Specific field filters
        if (!string.IsNullOrWhiteSpace(queryDto.FirstName))
        {
            query = query.Where(u => u.FirstName.ToLower().Contains(queryDto.FirstName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.LastName))
        {
            query = query.Where(u => u.LastName.ToLower().Contains(queryDto.LastName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Email))
        {
            query = query.Where(u => u.EmailAddress.ToLower().Contains(queryDto.Email.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.PhoneNumber))
        {
            query = query.Where(u => u.PhoneNumber.Contains(queryDto.PhoneNumber));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Gender))
        {
            query = query.Where(u => u.Gender.ToLower() == queryDto.Gender.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(queryDto.RoleName))
        {
            query = query.Where(u => u.UserInRoles.Any(ur => ur.Role.RoleName.ToLower().Contains(queryDto.RoleName.ToLower())));
        }

        // Date range filters
        if (queryDto.DateOfBirthFrom.HasValue)
        {
            query = query.Where(u => u.DateOfBirth >= queryDto.DateOfBirthFrom.Value);
        }

        if (queryDto.DateOfBirthTo.HasValue)
        {
            query = query.Where(u => u.DateOfBirth <= queryDto.DateOfBirthTo.Value);
        }

        if (queryDto.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= queryDto.CreatedFrom.Value);
        }

        if (queryDto.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= queryDto.CreatedTo.Value);
        }

        return query;
    }

    private IQueryable<UserDetails> ApplySorting(IQueryable<UserDetails> query, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "CreatedAt";

        var isDescending = sortDirection?.ToLower() == "desc";

        return sortBy.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "email" => isDescending ? query.OrderByDescending(u => u.EmailAddress) : query.OrderBy(u => u.EmailAddress),
            "dateofbirth" => isDescending ? query.OrderByDescending(u => u.DateOfBirth) : query.OrderBy(u => u.DateOfBirth),
            "gender" => isDescending ? query.OrderByDescending(u => u.Gender) : query.OrderBy(u => u.Gender),
            "createdat" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            "rolename" => isDescending
                ? query.OrderByDescending(u => u.UserInRoles.FirstOrDefault()!.Role.RoleName)
                : query.OrderBy(u => u.UserInRoles.FirstOrDefault()!.Role.RoleName),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };
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
        try
        {
            var existingUser = await _context.UserDetails.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new NotFoundException("User", user.Id);
            }

            // Check if email is being changed and if new email already exists
            if (existingUser.EmailAddress != user.EmailAddress)
            {
                var emailExists = await _context.UserDetails
                    .AnyAsync(u => u.EmailAddress == user.EmailAddress && u.Id != user.Id);

                if (emailExists)
                {
                    throw new ConflictException($"A user with email '{user.EmailAddress}' already exists.");
                }
            }

            user.UpdatedAt = DateTime.UtcNow;
            _context.UserDetails.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);
        }
        catch (Exception ex) when (!(ex is BaseException))
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", user.Id);
            throw;
        }
    }



    public async Task DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await _context.UserDetails.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User", id);
            }

            _context.UserDetails.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully with ID: {UserId}", id);
        }
        catch (Exception ex) when (!(ex is BaseException))
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            throw;
        }
    }
}
