using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        try
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
            return Ok(new { Message = "User created successfully", UserId = createdUser.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

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
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}