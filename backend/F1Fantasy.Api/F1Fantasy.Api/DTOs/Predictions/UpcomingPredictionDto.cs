namespace F1Fantasy.Api.DTOs.Predictions
{
    public class UpcomingPredictionDto
    {
        public PredictionRaceDto? Race { get; set; }
        public PredictionDto? ExistingPrediction { get; set; }

        public IReadOnlyList<PredictionDriverDto> AvailableDrivers { get; set; }
            = new List<PredictionDriverDto>();
    }
}
