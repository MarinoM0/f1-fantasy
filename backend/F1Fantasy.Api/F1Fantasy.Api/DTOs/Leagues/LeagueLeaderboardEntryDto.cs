namespace F1Fantasy.Api.DTOs.Leagues
{
    public class LeagueLeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public decimal TotalPoints { get; set; }
    }
}
