namespace F1Fantasy.Api.Models
{
    public class Race : BaseEntity
    {
        public int RoundNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CircuitName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public bool IsCompleted { get; set; }

        public RaceResult? RaceResult { get; set; }
        public ICollection<TeamScore> TeamScores { get; set; } = new List<TeamScore>();
    }
}
