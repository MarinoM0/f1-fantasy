namespace F1Fantasy.Api.Models
{
    public class LeagueMember
    {
        public int LeagueId { get; set; }
        public League League { get; set; } = null!;

        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsOwner { get; set; }
    }
}
