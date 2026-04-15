namespace F1Fantasy.Api.DTOs.Jolpica
{
    public class JolpicaRaceResultDto
    {
        public string Season { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;
        public string RaceName { get; set; } = string.Empty;
        public string CircuitId { get; set; } = string.Empty;
        public string CircuitName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime? RaceDateUtc { get; set; }

        public IReadOnlyList<JolpicaRaceResultEntryDto> Results { get; set; } = [];
    }

    public class JolpicaRaceResultEntryDto
    {
        public int? Position { get; set; }
        public string PositionText { get; set; } = string.Empty;
        public decimal Points { get; set; }

        public string DriverId { get; set; } = string.Empty;
        public string DriverCode { get; set; } = string.Empty;
        public string DriverFullName { get; set; } = string.Empty;

        public string ConstructorId { get; set; } = string.Empty;
        public string ConstructorName { get; set; } = string.Empty;

        public int? Grid { get; set; }
        public int? Laps { get; set; }
        public string Status { get; set; } = string.Empty;

        public bool FastestLap { get; set; }
        public string? FastestLapTime { get; set; }
        public string? RaceTime { get; set; }
    }
}
