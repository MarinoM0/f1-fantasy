namespace F1Fantasy.Api.Models
{
    public class Prediction : BaseEntity
    {
        public int UserId { get; set; }
        public AppUser User { get; set; } = null;

        public int RaceId { get; set; }
        public Race Race { get; set; } = null;

        public int P1DriverId { get; set; }
        public Driver P1Driver { get; set; } = null;

        public int P2DriverId { get; set; }
        public Driver P2Driver { get; set; } = null;

        public int P3DriverId { get; set; }
        public Driver P3Driver { get; set; } = null;

        public decimal? Score { get; set; }
        public bool IsScored { get; set; }
        public DateTime? ScoredAtUtc { get; set; }
    }
}
