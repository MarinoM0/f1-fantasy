using F1Fantasy.Api.DTOs.Predictions;
using F1Fantasy.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionsController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictionsController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(UpcomingPredictionDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UpcomingPredictionDto>> GetUpcoming(CancellationToken ct)
        {
            var result = await _predictionService.GetUpcomingAsync(GetUserId(), ct);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PredictionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PredictionDto>> Submit(
            CreatePredictionRequestDto request, CancellationToken ct)
        {
            var result = await _predictionService.SubmitAsync(GetUserId(), request, ct);
            return Ok(result);
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(IReadOnlyList<PredictionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<PredictionDto>>> GetMine(CancellationToken ct)
        {
            var result = await _predictionService.GetMyPredictionsAsync(GetUserId(), ct);
            return Ok(result);
        }

        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(IReadOnlyList<PredictionLeaderboardEntryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<PredictionLeaderboardEntryDto>>> GetLeaderboard(
            CancellationToken ct)
        {
            var result = await _predictionService.GetLeaderboardAsync(ct);
            return Ok(result);
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }
    }
}
