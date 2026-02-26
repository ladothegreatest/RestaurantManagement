using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Common.ApiResponses;
using RestaurantManagement.Dtos.Table;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TablesController : ControllerBase
    {
        private readonly ITableService _tablesService;
        private readonly ILogger<TablesController> _logger;

        public TablesController(ITableService tablesService, ILogger<TablesController> logger)
        {
            _tablesService = tablesService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tables = await _tablesService.GetAllAsync();
                return Ok(ApiResponsextensions<List<TableResponseDto>>.SuccessResponse(tables));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error occurred while fetching tables");
                return StatusCode(500, ApiResponsextensions<List<TableResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var table = await _tablesService.GetByIdAsync(id);

                if (table == null)
                    return NotFound(ApiResponsextensions<TableResponseDto>.FailureResponse("Table not found"));

                return Ok(ApiResponsextensions<TableResponseDto>.SuccessResponse(table));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching table with id {id}");
                return StatusCode(500, ApiResponsextensions<TableResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("by-restaurant/{restaurantId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByRestaurantId(int restaurantId)
        {
            try
            {
                var tables = await _tablesService.GetByRestaurantIdAsync(restaurantId);
                return Ok(ApiResponsextensions<List<TableResponseDto>>.SuccessResponse(tables));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"No tables found for restaurant with id {restaurantId}");
                return NotFound(ApiResponsextensions<List<TableResponseDto>>.FailureResponse(ex.Message));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching tables for restaurant with id {restaurantId}");
                return StatusCode(500, ApiResponsextensions<List<TableResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("available/{restaurantId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableTables(int restaurantId)
        {
            try
            {
                var tables = await _tablesService.GetAvailableTablesByRestaurantIdAsync(restaurantId);
                return Ok(ApiResponsextensions<List<TableResponseDto>>.SuccessResponse(tables));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"No available tables found for restaurant with id {restaurantId}");
                return NotFound(ApiResponsextensions<List<TableResponseDto>>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching available tables for restaurant with id {restaurantId}");
                return StatusCode(500, ApiResponsextensions<List<TableResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("by-capacity")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCapacity(int restaurantId, int minCapacity)
        {
            try
            {
                if (minCapacity <= 0)
                    return BadRequest(ApiResponsextensions<List<TableResponseDto>>.FailureResponse("Capacity must be greater than 0"));

                var tables = await _tablesService.GetByCapacityAsync(restaurantId, minCapacity);
                return Ok(ApiResponsextensions<List<TableResponseDto>>.SuccessResponse(tables));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"No tables found with capacity >= {minCapacity} for restaurant with id {restaurantId}");
                return NotFound(ApiResponsextensions<List<TableResponseDto>>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching tables with capacity >= {minCapacity} for restaurant with id {restaurantId}");
                return StatusCode(500, ApiResponsextensions<List<TableResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<TableResponseDto>.FailureResponse("Validation error"));

                var table = await _tablesService.CreateAsync(dto);
                return Ok(ApiResponsextensions<TableResponseDto>.SuccessResponse(table, "Table created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating table");
                return BadRequest(ApiResponsextensions<TableResponseDto>.FailureResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating table");
                return BadRequest(ApiResponsextensions<TableResponseDto>.FailureResponse(ex.Message));
            }
            catch
            {
                _logger.LogError("Unexpected error occurred while creating table"); 
                return StatusCode(500, ApiResponsextensions<TableResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTableDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<TableResponseDto>.FailureResponse("Validation error"));

                var table = await _tablesService.UpdateAsync(id, dto);

                if (table == null)
                    return NotFound(ApiResponsextensions<TableResponseDto>.FailureResponse("Table not found"));

                return Ok(ApiResponsextensions<TableResponseDto>.SuccessResponse(table, "Table updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Business rule violation while updating table with id {id}");
                return BadRequest(ApiResponsextensions<TableResponseDto>.FailureResponse(ex.Message));
            }
            catch
            {
                _logger.LogError($"Unexpected error occurred while updating table with id {id}");
                return StatusCode(500, ApiResponsextensions<TableResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPatch("{id}/toggle-availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            try
            {
                var success = await _tablesService.ToggleAvailabilityAsync(id);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("Table not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Table availability toggled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while toggling availability for table with id {id}");
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
                var success = await _tablesService.DeleteAsync(id);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("Table not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Table deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting table with id {id}");
                return StatusCode(500, ApiResponseExtensions.FailureResponse("Server error occurred"));
            }
        }
    }
}