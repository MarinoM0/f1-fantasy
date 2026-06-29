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
        private readonly IFantasyScoringService _fantasyScoringService;
        private readonly IPricingService _pricingService;

        public ResultsController(
            IResultsSyncService resultsSyncService,
            IFantasyScoringService fantasyScoringService,
            IPricingService pricingService)
        {
            _resultsSyncService = resultsSyncService;
            _fantasyScoringService = fantasyScoringService;
            _pricingService = pricingService;
        }

        // Pulls new results, scores them, then re-prices the market — the full
        // post-race pipeline in one call.
        [HttpPost("sync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Sync(CancellationToken ct)
        {
            var racesSynced = await _resultsSyncService.SyncCompletedRacesAsync(ct);
            var driversScored = await _fantasyScoringService.ScoreAllResultsAsync(ct);
            var pricesUpdated = await _pricingService.RecalculatePricesAsync(ct);

            return Ok(new { racesSynced, driversScored, pricesUpdated });
        }
    }
}
