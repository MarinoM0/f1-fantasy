using F1Fantasy.Api.DTOs.TeamBuilder;
using F1Fantasy.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamBuilderController : ControllerBase
    {
        private readonly TeamBuilderService _teamBuilderService;

        public TeamBuilderController(TeamBuilderService teamBuilderService)
        {
            _teamBuilderService = teamBuilderService;
        }

        [HttpGet("drivers")]
        [ProducesResponseType(typeof(IReadOnlyList<TeamBuilderDriverDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<TeamBuilderDriverDto>>> GetDrivers(CancellationToken cancellationToken)
        {
            var drivers = await _teamBuilderService.GetDriversAsync(cancellationToken);
            return Ok(drivers);
        }

        [HttpGet("constructors")]
        [ProducesResponseType(typeof(IReadOnlyList<TeamBuilderConstructorDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<TeamBuilderConstructorDto>>> GetConstructors(CancellationToken cancellationToken)
        {
            var constructors = await _teamBuilderService.GetConstructorsAsync(cancellationToken);
            return Ok(constructors);
        }
    }
}
