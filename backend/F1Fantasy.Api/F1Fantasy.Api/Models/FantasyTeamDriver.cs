namespace F1Fantasy.Api.Models
{
    public class FantasyTeamDriver
    {
        public int FantasyTeamId { get; set; }
        public FantasyTeam FantasyTeam { get; set; } = null!;

        public int DriverId { get; set; }
        public Driver Driver { get; set; } = null!;

        public decimal PointsAtTransfer { get; set; } = 0;
    }
}
