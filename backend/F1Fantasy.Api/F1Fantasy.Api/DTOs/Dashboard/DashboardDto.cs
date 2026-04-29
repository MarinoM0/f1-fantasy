namespace F1Fantasy.Api.DTOs.Dashboard
{
    public class DashboardDto
    {
        public DashboardUpcomingRaceDto? UpcomingRace { get; set; }

        public IReadOnlyList<DashboardLeaderboardEntryDto> Leaderboard { get; set; } = [];
    }
}
