using F1Fantasy.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly IResultsSyncService _resultsSyncService;

        public ResultsController(IResultsSyncService resultsSyncService)
        {
            _resultsSyncService = resultsSyncService;
        }

        [HttpPost("sync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Sync(CancellationToken ct)
        {
            var racesSynced = await _resultsSyncService.SyncCompletedRacesAsync(ct);
            return Ok(new { racesSynced });
        }
    }
}
