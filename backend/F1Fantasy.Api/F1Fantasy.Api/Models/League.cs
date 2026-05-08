namespace F1Fantasy.Api.Models
{
    public class League : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;

        public int OwnerId { get; set; }
        public AppUser Owner { get; set; } = null!;

        public ICollection<LeagueMember> Members { get; set; } = new List<LeagueMember>();
    }
}
