namespace F1Fantasy.Api.DTOs.Leagues
{
    public class LeagueDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string InviteCode { get; set; } = string.Empty;

        public int OwnerId { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
        public int MemberCount { get; set; }

        public bool IsOwner { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public List<LeagueMemberDto> Members { get; set; } = new();
    }
}
