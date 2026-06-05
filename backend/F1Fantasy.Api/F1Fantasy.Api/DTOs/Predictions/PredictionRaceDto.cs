namespace F1Fantasy.Api.DTOs.Predictions
{
    public class PredictionRaceDto
    {
        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }

        public bool IsLocked { get; set; }
        public bool IsCompleted { get; set; }
    }
}
