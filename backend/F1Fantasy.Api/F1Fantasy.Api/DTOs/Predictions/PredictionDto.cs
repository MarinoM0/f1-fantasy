namespace F1Fantasy.Api.DTOs.Predictions
{
    public class PredictionDto
    {
        public int Id { get; set; }
        public PredictionRaceDto Race { get; set; } = null;

        public PredictionDriverDto PredictedP1 { get; set; } = null!;
        public PredictionDriverDto PredictedP2 { get; set; } = null!;
        public PredictionDriverDto PredictedP3 { get; set; } = null!;

        public bool IsScored { get; set; }
        public decimal? Score { get; set; }

        public PredictionDriverDto? ActualP1 { get; set; }
        public PredictionDriverDto? ActualP2 { get; set; }
        public PredictionDriverDto? ActualP3 { get; set; }
    }
}
