using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

namespace MultitenantApiSingleDbSharedSchema.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _userService;


        public AdminUsersController(IAdminUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Creates a new user for the tenant.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUserAdminRequest request)
        {
            var result = await _userService.CreateAsync(
                request.Username,
                request.Password,
                request.DisplayName,
                request.Role);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User created successfully." });
            }

            return BadRequest(new { Message = "One or more validation errors occurred.", result.Errors });
        }

        /// <summary>
        /// Retrieves all users for the tenant.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();

            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by ID.
        /// </summary>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Deletes a user for the tenant.
        /// </summary>
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid userId)
        {
            var result = await _userService.DeleteAsync(userId);
            if (result.Succeeded)
            {
                return Ok(new { Message = "User deleted successfully." });
            }

            return NotFound(new
                { Message = $"User with ID '{userId}' was not found or you don't have permission to access it." });
        }
    }
}