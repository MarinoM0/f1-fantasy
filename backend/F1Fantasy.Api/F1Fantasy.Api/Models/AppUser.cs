namespace F1Fantasy.Api.Models
{
    public class AppUser : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public FantasyTeam? FantasyTeam { get; set; }
        public ICollection<TeamScore> TeamScores { get; set; } = new List<TeamScore>();
    }
}
