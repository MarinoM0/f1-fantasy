using System.Globalization;
using System.Net.Http.Json;
using F1Fantasy.Api.DTOs.Dashboard;
using F1Fantasy.Api.DTOs.Jolpica;
using F1Fantasy.Api.External.Jolpica;
using F1Fantasy.Api.Interfaces;

namespace F1Fantasy.Api.Services;

public class JolpicaService : IJolpicaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JolpicaService> _logger;

    public JolpicaService(HttpClient httpClient, ILogger<JolpicaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JolpicaDriverStandingDto>> GetDriverStandingsAsync(
        string season = "current",
        CancellationToken cancellationToken = default)
    {
        var response = await GetFromJolpicaAsync($"{season}/driverstandings.json", cancellationToken);

        var standingsTable = response.MRData.StandingsTable;
        var standingsList = standingsTable?.StandingsLists.FirstOrDefault();

        if (standingsTable is null || standingsList is null)
        {
            return [];
        }

        return standingsList.DriverStandings.Select(item =>
        {
            var constructor = item.Constructors.FirstOrDefault();

            return new JolpicaDriverStandingDto
            {
                Season = standingsTable.Season,
                Round = standingsTable.Round,
                Position = ParseInt(item.Position),
                PositionText = item.PositionText,
                Points = ParseDecimal(item.Points),
                Wins = ParseInt(item.Wins),
                DriverId = item.Driver.DriverId,
                Code = item.Driver.Code ?? string.Empty,
                GivenName = item.Driver.GivenName,
                FamilyName = item.Driver.FamilyName,
                FullName = $"{item.Driver.GivenName} {item.Driver.FamilyName}",
                ConstructorId = constructor?.ConstructorId ?? string.Empty,
                ConstructorName = constructor?.Name ?? string.Empty
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<JolpicaConstructorStandingDto>> GetConstructorStandingsAsync(
        string season = "current",
        CancellationToken cancellationToken = default)
    {
        var response = await GetFromJolpicaAsync($"{season}/constructorstandings.json", cancellationToken);

        var standingsTable = response.MRData.StandingsTable;
        var standingsList = standingsTable?.StandingsLists.FirstOrDefault();

        if (standingsTable is null || standingsList is null)
        {
            return [];
        }

        return standingsList.ConstructorStandings.Select(item =>
            new JolpicaConstructorStandingDto
            {
                Season = standingsTable.Season,
                Round = standingsTable.Round,
                Position = ParseInt(item.Position),
                PositionText = item.PositionText,
                Points = ParseDecimal(item.Points),
                Wins = ParseInt(item.Wins),
                ConstructorId = item.Constructor.ConstructorId,
                ConstructorName = item.Constructor.Name
            }).ToList();
    }

    public async Task<JolpicaRaceResultDto?> GetRaceResultsAsync(
        string season = "current",
        string round = "last",
        CancellationToken cancellationToken = default)
    {
        var response = await GetFromJolpicaAsync($"{season}/{round}/results.json", cancellationToken);

        var race = response.MRData.RaceTable?.Races.FirstOrDefault();
        if (race is null)
        {
            return null;
        }

        return new JolpicaRaceResultDto
        {
            Season = race.Season,
            Round = race.Round,
            RaceName = race.RaceName,
            CircuitId = race.Circuit.CircuitId,
            CircuitName = race.Circuit.CircuitName,
            Country = race.Circuit.Location.Country,
            RaceDateUtc = ParseRaceDateUtc(race.Date, race.Time),
            Results = race.Results.Select(result => new JolpicaRaceResultEntryDto
            {
                Position = ParseNullableInt(result.Position),
                PositionText = result.PositionText,
                Points = ParseDecimal(result.Points),
                DriverId = result.Driver.DriverId,
                DriverCode = result.Driver.Code ?? string.Empty,
                DriverFullName = $"{result.Driver.GivenName} {result.Driver.FamilyName}",
                ConstructorId = result.Constructor.ConstructorId,
                ConstructorName = result.Constructor.Name,
                Grid = ParseNullableInt(result.Grid),
                Laps = ParseNullableInt(result.Laps),
                Status = result.Status,
                FastestLap = string.Equals(result.FastestLap?.Rank, "1", StringComparison.Ordinal),
                FastestLapTime = result.FastestLap?.Time?.Time,
                RaceTime = result.Time?.Time
            }).ToList()
        };
    }

    public async Task<DashboardUpcomingRaceDto?> GetNextRaceAsync (
        string season = "current",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await GetFromJolpicaAsync($"{season}/next/races.json", cancellationToken);

            var raceTable = response.MRData.RaceTable;
            var race = raceTable?.Races.FirstOrDefault();

            if (race is null)
            {
                return null;
            }

            return new DashboardUpcomingRaceDto
            {
                LocalRaceId = null,
                Season = race.Season,
                RoundNumber = ParseInt(race.Round),
                RaceName = race.RaceName,
                CircuitId = race.Circuit.CircuitId,
                CircuitName = race.Circuit.CircuitName,
                Country = race.Circuit.Location.Country,
                Locality = race.Circuit.Location.Locality,
                StartTimeUtc = ParseRaceDateUtc(race.Date, race.Time),
                DataSource = "jolpica"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch next race from Jolpica");
            return null;
        }
    }

    private async Task<JolpicaResponse> GetFromJolpicaAsync(
        string relativeUrl,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JolpicaResponse>(cancellationToken: cancellationToken);

        if (payload is null)
        {
            _logger.LogError("Jolpica returned an empty payload for {RelativeUrl}", relativeUrl);
            throw new InvalidOperationException("Jolpica returned an empty payload.");
        }

        return payload;
    }

    private static int ParseInt(string? value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    private static int? ParseNullableInt(string? value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static decimal ParseDecimal(string? value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;

    private static DateTime? ParseRaceDateUtc(string? date, string? time)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return null;
        }

        var combined = string.IsNullOrWhiteSpace(time)
            ? $"{date}T00:00:00Z"
            : $"{date}T{time}";

        return DateTime.TryParse(
            combined,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
            out var parsed)
            ? parsed
            : null;
    }
}