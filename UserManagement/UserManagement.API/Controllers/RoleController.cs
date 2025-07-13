using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Interfaces;
using UserManagement.Domain.Entities;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto roleDto)
    {
        try
        {
            var role = new Role { RoleName = roleDto.RoleName };
            var createdRole = await _roleService.CreateRoleAsync(role);
            return Ok(new { Message = "Role created successfully", RoleId = createdRole.RoleID });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
            return NotFound();

        return Ok(role);
    }
}

public class CreateRoleDto
{
    public string RoleName { get; set; } = string.Empty;
}