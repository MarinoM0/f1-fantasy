using F1Fantasy.Api.DTOs;
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
    public class FantasyTeamController : ControllerBase
    {
        private readonly IFantasyTeamService _fantasyTeamService;

        public FantasyTeamController(IFantasyTeamService fantasyTeamService)
        {
            _fantasyTeamService = fantasyTeamService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(FantasyTeamDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<FantasyTeamDto>> Create(CreateFantasyTeamRequestDto request)
        {
            var userId = GetUserId();
            var team = await _fantasyTeamService.CreateAsync(userId, request);
            return Ok(team);
        }

        [HttpPut]
        [ProducesResponseType(typeof(FantasyTeamDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FantasyTeamDto>> Update(CreateFantasyTeamRequestDto request)
        {
            var userId = GetUserId();
            var team = await _fantasyTeamService.UpdateAsync(userId, request);
            return Ok(team);
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(FantasyTeamDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FantasyTeamDto>> GetMyTeam()
        {
            var userId = GetUserId();
            var team = await _fantasyTeamService.GetMyTeamAsync(userId);

            if (team is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = "Fantasy team not found.",
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(team);
        }


        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var UserId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            return UserId;
        }
    }
}
