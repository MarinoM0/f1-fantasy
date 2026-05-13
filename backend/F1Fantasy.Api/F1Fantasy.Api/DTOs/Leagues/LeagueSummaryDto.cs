namespace F1Fantasy.Api.DTOs.Leagues
{
    public class LeagueSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public bool IsOwner { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
