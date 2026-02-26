using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Dtos.Reservation;
using RestaurantManagement.Services;

namespace RestaurantManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly ReservationService _service;

    public ReservationController(ReservationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.CreateAsync(dto);
        return StatusCode(201);
    }
}