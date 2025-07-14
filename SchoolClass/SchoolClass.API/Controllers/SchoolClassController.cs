using Microsoft.AspNetCore.Mvc;
using SchoolClass.Application.DTOs;
using SchoolClass.Application.Interfaces;
using SchoolClass.Domain.Entities;

namespace SchoolClass.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolClassController : ControllerBase
{
    private readonly ISchoolClassService _schoolClassService;
    private readonly ILogger<SchoolClassController> _logger;

    public SchoolClassController(ISchoolClassService schoolClassService, ILogger<SchoolClassController> logger)
    {
        _schoolClassService = schoolClassService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new school class
    /// </summary>
    /// <param name="createDto">School class creation data</param>
    /// <returns>Created school class</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SchoolClassResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SchoolClassResponseDto>> CreateSchoolClass([FromBody] CreateSchoolClassDto createDto)
    {
        try
        {
            _logger.LogInformation("Creating new school class with name: {SchoolClassName}", createDto.SchoolClassName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Map DTO to Domain Entity
            var schoolClass = new Domain.Entities.SchoolClasse
            {
                SchoolClassName = createDto.SchoolClassName
            };

            var createdSchoolClass = await _schoolClassService.CreateSchoolClassAsync(schoolClass);

            // Map Domain Entity to Response DTO
            var responseDto = new SchoolClassResponseDto
            {
                SchoolClassID = createdSchoolClass.SchoolClassID,
                SchoolClassName = createdSchoolClass.SchoolClassName,
                CreatedAt = createdSchoolClass.CreatedAt,
                UpdatedAt = createdSchoolClass.UpdatedAt
            };

            _logger.LogInformation("Successfully created school class with ID: {SchoolClassID}", createdSchoolClass.SchoolClassID);

            return CreatedAtAction(nameof(GetSchoolClassById), new { id = createdSchoolClass.SchoolClassID }, responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating school class with name: {SchoolClassName}", createDto.SchoolClassName);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the school class");
        }
    }

    /// <summary>
    /// Retrieves a school class by ID
    /// </summary>
    /// <param name="id">School class ID</param>
    /// <returns>School class details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SchoolClassResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SchoolClassResponseDto>> GetSchoolClassById(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving school class with ID: {SchoolClassID}", id);

            if (id == Guid.Empty)
            {
                return BadRequest("Invalid school class ID");
            }

            var schoolClass = await _schoolClassService.GetSchoolClassByIdAsync(id);

            if (schoolClass == null)
            {
                _logger.LogWarning("School class with ID: {SchoolClassID} not found", id);
                return NotFound($"School class with ID {id} not found");
            }

            var responseDto = new SchoolClassResponseDto
            {
                SchoolClassID = schoolClass.SchoolClassID,
                SchoolClassName = schoolClass.SchoolClassName,
                CreatedAt = schoolClass.CreatedAt,
                UpdatedAt = schoolClass.UpdatedAt
            };

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving school class with ID: {SchoolClassID}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the school class");
        }
    }

    /// <summary>
    /// Retrieves all school classes
    /// </summary>
    /// <returns>List of all school classes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SchoolClassResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<SchoolClassResponseDto>>> GetAllSchoolClasses()
    {
        try
        {
            _logger.LogInformation("Retrieving all school classes");

            var schoolClasses = await _schoolClassService.GetAllSchoolClassesAsync();

            var responseDtos = schoolClasses.Select(sc => new SchoolClassResponseDto
            {
                SchoolClassID = sc.SchoolClassID,
                SchoolClassName = sc.SchoolClassName,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} school classes", responseDtos.Count);

            return Ok(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all school classes");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving school classes");
        }
    }

    /// <summary>
    /// Updates an existing school class
    /// </summary>
    /// <param name="id">School class ID</param>
    /// <param name="updateDto">Updated school class data</param>
    /// <returns>Updated school class</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SchoolClassResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SchoolClassResponseDto>> UpdateSchoolClass(Guid id, [FromBody] CreateSchoolClassDto updateDto)
    {
        try
        {
            _logger.LogInformation("Updating school class with ID: {SchoolClassID}", id);

            if (id == Guid.Empty)
            {
                return BadRequest("Invalid school class ID");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if school class exists
            var existingSchoolClass = await _schoolClassService.GetSchoolClassByIdAsync(id);
            if (existingSchoolClass == null)
            {
                _logger.LogWarning("School class with ID: {SchoolClassID} not found for update", id);
                return NotFound($"School class with ID {id} not found");
            }

            // Update the entity
            existingSchoolClass.SchoolClassName = updateDto.SchoolClassName;

            await _schoolClassService.UpdateSchoolClassAsync(existingSchoolClass);

            var responseDto = new SchoolClassResponseDto
            {
                SchoolClassID = existingSchoolClass.SchoolClassID,
                SchoolClassName = existingSchoolClass.SchoolClassName,
                CreatedAt = existingSchoolClass.CreatedAt,
                UpdatedAt = existingSchoolClass.UpdatedAt
            };

            _logger.LogInformation("Successfully updated school class with ID: {SchoolClassID}", id);

            return Ok(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating school class with ID: {SchoolClassID}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the school class");
        }
    }

    /// <summary>
    /// Deletes a school class
    /// </summary>
    /// <param name="id">School class ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteSchoolClass(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting school class with ID: {SchoolClassID}", id);

            if (id == Guid.Empty)
            {
                return BadRequest("Invalid school class ID");
            }

            // Check if school class exists
            var existingSchoolClass = await _schoolClassService.GetSchoolClassByIdAsync(id);
            if (existingSchoolClass == null)
            {
                _logger.LogWarning("School class with ID: {SchoolClassID} not found for deletion", id);
                return NotFound($"School class with ID {id} not found");
            }

            await _schoolClassService.DeleteSchoolClassAsync(id);

            _logger.LogInformation("Successfully deleted school class with ID: {SchoolClassID}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting school class with ID: {SchoolClassID}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the school class");
        }
    }
}