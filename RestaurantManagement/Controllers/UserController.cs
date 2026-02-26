using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestaurantManagement.Common.ApiResponses;
using RestaurantManagement.Dtos.User;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _usersService;
        private readonly ILogger<UsersController> _logger;
        public UsersController(IUserService usersService, ILogger<UsersController> logger)
        {
            _usersService = usersService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<LoginResponseDto>.FailureResponse("Validation error"));

                var result = await _usersService.RegisterAsync(dto);
                return Ok(ApiResponsextensions<LoginResponseDto>.SuccessResponse(result, "User registered successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for email {Email}", dto.Email); 
                return BadRequest(ApiResponsextensions<LoginResponseDto>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration for email {Email}", dto.Email);
                return StatusCode(500, ApiResponsextensions<LoginResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<LoginResponseDto>.FailureResponse("Validation error"));

                var result = await _usersService.LoginAsync(dto);
                return Ok(ApiResponsextensions<LoginResponseDto>.SuccessResponse(result, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed for email {Email}", dto.Email);
                return Unauthorized(ApiResponsextensions<LoginResponseDto>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for email {Email}", dto.Email);
                return StatusCode(500, ApiResponsextensions<LoginResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _usersService.GetAllAsync();
                return Ok(ApiResponsextensions<List<UserResponseDto>>.SuccessResponse(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users");
                return StatusCode(500, ex.Message + " | " + ex.InnerException?.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _usersService.GetByIdAsync(id);

                if (user == null)
                    return NotFound(ApiResponsextensions<UserResponseDto>.FailureResponse("User not found"));

                return Ok(ApiResponsextensions<UserResponseDto>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user with ID {UserId}", id);
                return StatusCode(500, ApiResponsextensions<UserResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("email/{email}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var user = await _usersService.GetByEmailAsync(email);

                if (user == null)
                    return NotFound(ApiResponsextensions<UserResponseDto>.FailureResponse("User not found"));

                return Ok(ApiResponsextensions<UserResponseDto>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user with email {Email}", email);
                return StatusCode(500, ApiResponsextensions<UserResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<UserResponseDto>.FailureResponse("Validation error"));

                var user = await _usersService.UpdateAsync(id, dto);

                if (user == null)
                    return NotFound(ApiResponsextensions<UserResponseDto>.FailureResponse("User not found"));

                return Ok(ApiResponsextensions<UserResponseDto>.SuccessResponse(user, "User updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Update failed for user ID {UserId}", id);
                return BadRequest(ApiResponsextensions<UserResponseDto>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user with ID {UserId}", id);
                return StatusCode(500, ApiResponsextensions<UserResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPut("{id}/change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseExtensions.FailureResponse("Validation error"));

                var success = await _usersService.ChangePasswordAsync(id, dto);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("User not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Password changed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password change failed for user ID {UserId}", id);
                return BadRequest(ApiResponseExtensions.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password for user ID {UserId}", id);
                return StatusCode(500, ApiResponseExtensions.FailureResponse("Server error occurred"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _usersService.DeleteAsync(id);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("User not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("User deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with ID {UserId}", id);
                return StatusCode(500, ApiResponseExtensions.FailureResponse("Server error occurred"));
            }
        }
    }
}