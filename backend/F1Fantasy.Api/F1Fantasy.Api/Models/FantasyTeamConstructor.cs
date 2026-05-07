namespace F1Fantasy.Api.Models
{
    public class FantasyTeamConstructor
    {
        public int FantasyTeamId { get; set; }
        public FantasyTeam FantasyTeam { get; set; } = null!;

        public int ConstructorId { get; set; }
        public Constructor Constructor { get; set; } = null!;

        public decimal PointsAtTransfer { get; set; } = 0;
    }
}