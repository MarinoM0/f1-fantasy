using F1Fantasy.Api.DTOs.Leagues;
using F1Fantasy.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace F1Fantasy.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaguesController : ControllerBase
    {
        private readonly ILeagueService _leagueService;

        public LeaguesController(ILeagueService leagueService)
        {
            _leagueService = leagueService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<LeagueDto>> Create(
            CreateLeagueRequestDto request, CancellationToken ct)
        {
            var league = await _leagueService.CreateAsync(GetUserId(), request, ct);
            return Ok(league);
        }

        [HttpPost("join")]
        [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LeagueDto>> Join(
           JoinLeagueRequestDto request, CancellationToken ct)
        {
            var league = await _leagueService.JoinAsync(GetUserId(), request, ct);
            return Ok(league);
        }

        [HttpPost("{id:int}/leave")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Leave(int id, CancellationToken ct)
        {
            await _leagueService.LeaveAsync(GetUserId(), id, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _leagueService.DeleteAsync(GetUserId(), id, ct);
            return NoContent();
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(IReadOnlyList<LeagueSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<LeagueSummaryDto>>> GetMine(
            CancellationToken ct)
        {
            var leagues = await _leagueService.GetMyLeaguesAsync(GetUserId(), ct);
            return Ok(leagues);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LeagueDto>> GetById(int id, CancellationToken ct)
        {
            var league = await _leagueService.GetByIdAsync(GetUserId(), id, ct);
            return Ok(league);
        }

        [HttpGet("{id:int}/leaderboard")]
        [ProducesResponseType(typeof(IReadOnlyList<LeagueLeaderboardEntryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<LeagueLeaderboardEntryDto>>> GetLeaderboard(
           int id, CancellationToken ct)
        {
            var leaderboard = await _leagueService.GetLeaderboardAsync(GetUserId(), id, ct);
            return Ok(leaderboard);
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
