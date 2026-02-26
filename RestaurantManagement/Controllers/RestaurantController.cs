using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Dtos.Restaurant;
using RestaurantManagement.Services;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantController : ControllerBase
    {
        private readonly RestaurantService _service;

        public RestaurantController(RestaurantService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurants = await _service.GetAllAsync();
            return Ok(restaurants);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRestaurantDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.CreateAsync(dto);
            return StatusCode(201);
        }
    }
}