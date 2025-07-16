// UserManagement/UserManagement.API/Controllers/UserController.cs (Updated)
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Application.Exceptions;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        var user = new UserDetails
        {
            FirstName = userDto.FirstName,
            MiddleName = userDto.MiddleName,
            LastName = userDto.LastName,
            DateOfBirth = userDto.DateOfBirth,
            Gender = userDto.Gender,
            EmailAddress = userDto.EmailAddress,
            Password = userDto.Password,
            PhoneNumber = userDto.PhoneNumber
        };

        var createdUser = await _userService.CreateUserAsync(user, userDto.RoleId);
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id },
            new { Message = "User created successfully", UserId = createdUser.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        var userResponse = new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            EmailAddress = user.EmailAddress,
            PhoneNumber = user.PhoneNumber,
            RoleName = user.UserInRoles.FirstOrDefault()?.Role?.RoleName ?? "",
            CreatedAt = user.CreatedAt
        };

        return Ok(userResponse);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryDto queryDto)
    {
        var result = await _userService.GetUsersAsync(queryDto);

        // Add pagination metadata to response headers
        Response.Headers.Add("X-Pagination-TotalCount", result.TotalCount.ToString());
        Response.Headers.Add("X-Pagination-TotalPages", result.TotalPages.ToString());
        Response.Headers.Add("X-Pagination-CurrentPage", result.Page.ToString());
        Response.Headers.Add("X-Pagination-PageSize", result.PageSize.ToString());
        Response.Headers.Add("X-Pagination-HasPrevious", result.HasPreviousPage.ToString());
        Response.Headers.Add("X-Pagination-HasNext", result.HasNextPage.ToString());

        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var userResponses = users.Select(user => new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            EmailAddress = user.EmailAddress,
            PhoneNumber = user.PhoneNumber,
            RoleName = user.UserInRoles.FirstOrDefault()?.Role?.RoleName ?? "",
            CreatedAt = user.CreatedAt
        }).ToList();

        return Ok(userResponses);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCredentials([FromBody] LoginDto loginDto)
    {
        var isValid = await _userService.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password);

        if (isValid)
        {
            var user = await _userService.GetUserByEmailAsync(loginDto.Email);
            return Ok(new { Message = "Valid credentials", UserId = user?.Id });
        }

        return Unauthorized(new { Message = "Invalid credentials" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto userDto)
    {
        var user = await _userService.GetUserByIdAsync(id);

        // Update user properties
        user.FirstName = userDto.FirstName;
        user.MiddleName = userDto.MiddleName;
        user.LastName = userDto.LastName;
        user.DateOfBirth = userDto.DateOfBirth;
        user.Gender = userDto.Gender;
        user.EmailAddress = userDto.EmailAddress;
        user.PhoneNumber = userDto.PhoneNumber;

        await _userService.UpdateUserAsync(user);
        return Ok(new { Message = "User updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(new { Message = "User deleted successfully" });
    }
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}