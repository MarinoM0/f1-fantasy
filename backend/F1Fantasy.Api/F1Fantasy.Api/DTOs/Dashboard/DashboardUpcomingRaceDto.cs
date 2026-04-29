namespace F1Fantasy.Api.DTOs.Dashboard
{
    public class DashboardUpcomingRaceDto
    {
        public int? LocalRaceId { get; set; }

        public string Season { get; set; } = string.Empty;

        public int RoundNumber { get; set; }

        public string RaceName { get; set; } = string.Empty;

        public string CircuitId { get; set; } = string.Empty;

        public string CircuitName { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Locality { get; set; } = string.Empty;

        public DateTime? StartTimeUtc { get; set; }

        public string DataSource { get; set; } = "local";
    }
}
