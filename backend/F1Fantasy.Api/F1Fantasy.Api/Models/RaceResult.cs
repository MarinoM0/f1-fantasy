namespace F1Fantasy.Api.Models
{
    public class RaceResult : BaseEntity
    {
        public int RaceId { get; set; }
        public Race Race { get; set; } = null!;

        public ICollection<RaceResultDriver> DriverResults { get; set; } = new List<RaceResultDriver>();
    }
}
