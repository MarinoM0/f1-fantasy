using F1Fantasy.Api.DTOs.Jolpica;
using F1Fantasy.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace F1Fantasy.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class JolpicaController : ControllerBase
{
    private readonly IJolpicaService _jolpicaService;

    public JolpicaController(IJolpicaService jolpicaService)
    {
        _jolpicaService = jolpicaService;
    }

    [HttpGet("driver-standings")]
    [ProducesResponseType(typeof(IReadOnlyList<JolpicaDriverStandingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JolpicaDriverStandingDto>>> GetDriverStandings(
        [FromQuery] string season = "current",
        CancellationToken cancellationToken = default)
    {
        var standings = await _jolpicaService.GetDriverStandingsAsync(season, cancellationToken);
        return Ok(standings);
    }

    [HttpGet("constructor-standings")]
    [ProducesResponseType(typeof(IReadOnlyList<JolpicaConstructorStandingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JolpicaConstructorStandingDto>>> GetConstructorStandings(
        [FromQuery] string season = "current",
        CancellationToken cancellationToken = default)
    {
        var standings = await _jolpicaService.GetConstructorStandingsAsync(season, cancellationToken);
        return Ok(standings);
    }

    [HttpGet("race-results")]
    [ProducesResponseType(typeof(JolpicaRaceResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JolpicaRaceResultDto>> GetRaceResults(
        [FromQuery] string season = "current",
        [FromQuery] string round = "last",
        CancellationToken cancellationToken = default)
    {
        var result = await _jolpicaService.GetRaceResultsAsync(season, round, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}