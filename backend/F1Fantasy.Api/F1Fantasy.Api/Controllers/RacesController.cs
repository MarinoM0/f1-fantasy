using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs;
using F1Fantasy.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RacesController : ControllerBase
    {
        private readonly RaceService _raceService;

        public RacesController (RaceService raceService)
        {
            _raceService = raceService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RaceDto>), StatusCodes.Status200OK)]
        public async Task <ActionResult<IReadOnlyList<RaceDto>>> GetAll()
        {
            var races = await _raceService.GetAllAsync();
            return Ok(races);
        }
    }
}
