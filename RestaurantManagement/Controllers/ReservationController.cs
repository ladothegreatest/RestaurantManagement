using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Common;
using RestaurantManagement.Common.ApiResponses;
using RestaurantManagement.Common.Enums;
using RestaurantManagement.Dtos.Reservation;
using RestaurantManagement.Services.Interfaces;

namespace RestaurantManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationsService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(IReservationService reservationsService, ILogger<ReservationsController> logger)
        {
            _reservationsService = reservationsService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var reservations = await _reservationsService.GetAllAsync();
                return Ok(ApiResponsextensions<List<ReservationResponseDto>>.SuccessResponse(reservations));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all reservations");
                return StatusCode(500, ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Server error occurred"));
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
                var reservation = await _reservationsService.GetByIdAsync(id);

                if (reservation == null)
                    return NotFound(ApiResponsextensions<ReservationResponseDto>.FailureResponse("Reservation not found"));

                return Ok(ApiResponsextensions<ReservationResponseDto>.SuccessResponse(reservation));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching reservation with ID {id}");
                return StatusCode(500, ApiResponsextensions<ReservationResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("by-user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var reservations = await _reservationsService.GetByUserIdAsync(userId);
                return Ok(ApiResponsextensions<List<ReservationResponseDto>>.SuccessResponse(reservations));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"No reservations found for user ID {userId}");
                return NotFound(ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching reservations for user ID {userId}");
                return StatusCode(500, ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("by-restaurant/{restaurantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByRestaurantId(int restaurantId)
        {
            try
            {
                var reservations = await _reservationsService.GetByRestaurantIdAsync(restaurantId);
                return Ok(ApiResponsextensions<List<ReservationResponseDto>>.SuccessResponse(reservations));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"No reservations found for restaurant ID {restaurantId}");
                return NotFound(ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse(ex.Message));
            }
            catch
            {
                _logger.LogError($"An error occurred while fetching reservations for restaurant ID {restaurantId}");
                return StatusCode(500, ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("by-table/{tableId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByTableId(int tableId)
        {
            try
            {
                var reservations = await _reservationsService.GetByTableIdAsync(tableId);
                return Ok(ApiResponsextensions<List<ReservationResponseDto>>.SuccessResponse(reservations));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching reservations for table ID {tableId}");
                return StatusCode(500, ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("by-date-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest(ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Start date must be before end date"));

                var reservations = await _reservationsService.GetByDateRangeAsync(startDate, endDate);
                return Ok(ApiResponsextensions<List<ReservationResponseDto>>.SuccessResponse(reservations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching reservations for date range {startDate} - {endDate}");
                return StatusCode(500, ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("by-status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByStatus(string status)
        {
            try
            {
                if (!Enum.TryParse<ReservationStatus>(status, true, out var reservationStatus))
                    return BadRequest(ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Invalid status"));

                var reservations = await _reservationsService.GetByStatusAsync(reservationStatus);
                return Ok(ApiResponsextensions<List<ReservationResponseDto>>.SuccessResponse(reservations));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching reservations with status {status}");
                return StatusCode(500, ApiResponsextensions<List<ReservationResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                    return BadRequest(ApiResponsextensions<PagedResult<ReservationResponseDto>>.FailureResponse("Invalid pagination parameters"));

                var result = await _reservationsService.GetPagedAsync(page, pageSize);
                return Ok(ApiResponsextensions<PagedResult<ReservationResponseDto>>.SuccessResponse(result, "Paged data loaded"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching paged reservations for page {page} and page size {pageSize}");
                return StatusCode(500, ApiResponsextensions<PagedResult<ReservationResponseDto>>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<ReservationResponseDto>.FailureResponse("Validation error"));

                var reservation = await _reservationsService.CreateAsync(dto);
                return Ok(ApiResponsextensions<ReservationResponseDto>.SuccessResponse(reservation, "Reservation created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating reservation");
                return BadRequest(ApiResponsextensions<ReservationResponseDto>.FailureResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating reservation");
                return BadRequest(ApiResponsextensions<ReservationResponseDto>.FailureResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a reservation");
                return StatusCode(500, ApiResponsextensions<ReservationResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponsextensions<ReservationResponseDto>.FailureResponse("Validation error"));

                var reservation = await _reservationsService.UpdateAsync(id, dto);

                if (reservation == null)
                    return NotFound(ApiResponsextensions<ReservationResponseDto>.FailureResponse("Reservation not found"));

                return Ok(ApiResponsextensions<ReservationResponseDto>.SuccessResponse(reservation, "Reservation updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Business rule violation while updating reservation with ID {id}");
                return BadRequest(ApiResponsextensions<ReservationResponseDto>.FailureResponse(ex.Message));
            }
            catch
            {
                return StatusCode(500, ApiResponsextensions<ReservationResponseDto>.FailureResponse("Server error occurred"));
            }
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReservationStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponseExtensions.FailureResponse("Validation error"));

                var success = await _reservationsService.UpdateStatusAsync(id, dto);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("Reservation not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Reservation status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating status for reservation with ID {id}");
                return StatusCode(500, ApiResponseExtensions.FailureResponse("Server error occurred"));
            }
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var success = await _reservationsService.CancelAsync(id);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("Reservation not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Reservation cancelled successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Business rule violation while cancelling reservation with ID {id}");
                return BadRequest(ApiResponseExtensions.FailureResponse(ex.Message));
            }
            catch
            {
                _logger.LogError($"An error occurred while cancelling reservation with ID {id}");
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
                var success = await _reservationsService.DeleteAsync(id);

                if (!success)
                    return NotFound(ApiResponseExtensions.FailureResponse("Reservation not found"));

                return Ok(ApiResponseExtensions.SuccessResponse("Reservation deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting reservation with ID {id}");
                return StatusCode(500, ApiResponseExtensions.FailureResponse("Server error occurred"));
            }
        }
    }
}