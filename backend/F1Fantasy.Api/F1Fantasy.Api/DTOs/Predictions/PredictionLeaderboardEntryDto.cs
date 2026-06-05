namespace F1Fantasy.Api.DTOs.Predictions
{
    public class PredictionLeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal TotalPoints { get; set; }
        public int PredictionsScored { get; set; }
    }
}
