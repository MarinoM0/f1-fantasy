namespace F1Fantasy.Api.DTOs.Leagues
{
    public class LeagueMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        public string? TeamName { get; set; }

        public bool IsOwner { get; set; }
        public DateTime JoinedAtUtc { get; set; }
    }
}
