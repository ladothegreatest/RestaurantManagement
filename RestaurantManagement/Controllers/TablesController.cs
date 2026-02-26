using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Dtos.Table;
using RestaurantManagement.Services;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly TableService _service;

        public TablesController(TableService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.CreateAsync(dto);
            return StatusCode(201);
        }
    }
}