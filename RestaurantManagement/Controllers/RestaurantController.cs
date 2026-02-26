using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Common.ApiResponses;
using RestaurantManagement.Dtos.Restaurant;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RestaurantsController : ControllerBase
    {
        private readonly IRestaurantService _restaurantsService;
        private readonly ILogger<RestaurantsController> _logger;

        public RestaurantsController(IRestaurantService restaurantsService, ILogger<RestaurantsController> logger)
        {
            _restaurantsService = restaurantsService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var restaurants = await _restaurantsService.GetAllAsync();
                return Ok(ApiResponsextensions<List<RestaurantResponseDto>>.SuccessResponse(restaurants));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all restaurants");
                return StatusCode(500, ApiResponsextensions<List<RestaurantResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var restaurant = await _restaurantsService.GetByIdAsync(id);

                if (restaurant == null)
                    return NotFound(ApiResponsextensions<RestaurantResponseDto>.FailureResponse("Restaurant not found"));

                return Ok(ApiResponsextensions<RestaurantResponseDto>.SuccessResponse(restaurant));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching restaurant with ID {id}");
                return StatusCode(500, ApiResponsextensions<RestaurantResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("open")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOpenRestaurants()
        {
            try
            {
                var restaurants = await _restaurantsService.GetOpenRestaurantsAsync();
                return Ok(ApiResponsextensions<List<RestaurantResponseDto>>.SuccessResponse(restaurants));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching open restaurants");
                return StatusCode(500, ApiResponsextensions<List<RestaurantResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("search/{name}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchByName(string name)
        {
            try
            {
                var restaurants = await _restaurantsService.GetByNameAsync(name);

                if (restaurants == null || !restaurants.Any())
                    return NotFound(ApiResponsextensions<List<RestaurantResponseDto>>.FailureResponse("No restaurants found"));

                return Ok(ApiResponsextensions<List<RestaurantResponseDto>>.SuccessResponse(restaurants));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while searching for restaurants with name '{name}'");
                return StatusCode(500, ApiResponsextensions<List<RestaurantResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateRestaurantDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<RestaurantResponseDto>.FailureResponse("Validation error"));

                var restaurant = await _restaurantsService.CreateAsync(dto);
                return Ok(ApiResponsextensions<RestaurantResponseDto>.SuccessResponse(restaurant, "Restaurant created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new restaurant");
                return StatusCode(500, ApiResponsextensions<RestaurantResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRestaurantDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<RestaurantResponseDto>.FailureResponse("Validation error"));

                var restaurant = await _restaurantsService.UpdateAsync(id, dto);

                if (restaurant == null)
                    return NotFound(ApiResponsextensions<RestaurantResponseDto>.FailureResponse("Restaurant not found"));

                return Ok(ApiResponsextensions<RestaurantResponseDto>.SuccessResponse(restaurant, "Restaurant updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating restaurant with ID {id}");
                return StatusCode(500, ApiResponsextensions<RestaurantResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleOpenStatus(int id)
        {
            try
            {
                var success = await _restaurantsService.ToggleOpenStatusAsync(id);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("Restaurant not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Restaurant status toggled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while toggling open status for restaurant with ID {id}");
                return StatusCode(500, ApiResponseExtensions.FailureResponse("Server error occurred"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _restaurantsService.DeleteAsync(id);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("Restaurant not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Restaurant deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting restaurant with ID {id}"); 
                return StatusCode(500, ApiResponseExtensions.FailureResponse("Server error occurred"));
            }
        }
    }
}