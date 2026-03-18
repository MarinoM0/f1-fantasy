using F1Fantasy.Api.DTOs;
using F1Fantasy.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly DriverService _driverService;

        public DriversController (DriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<DriverDto>), StatusCodes.Status200OK)]
        public async Task <ActionResult<IReadOnlyList<DriverDto>>> GetAll()
        {
            var drivers = await _driverService.GetAllAsync();
            return Ok(drivers);
        }
    }
}
