
// ================================
// 3. Updated Service Implementation
// ================================

// UserManagement/UserManagement.Infrastructure/Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Data;
using BCrypt.Net;
using System.Linq.Expressions;

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

    public async Task<PagedResultDto<UserSummaryDto>> GetUsersAsync(UserQueryDto queryDto)
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
