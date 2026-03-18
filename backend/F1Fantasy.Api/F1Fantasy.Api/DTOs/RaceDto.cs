namespace F1Fantasy.Api.DTOs
{
    public class RaceDto
    {
        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CircuitName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public bool IsCompleted { get; set; }
    }
}
