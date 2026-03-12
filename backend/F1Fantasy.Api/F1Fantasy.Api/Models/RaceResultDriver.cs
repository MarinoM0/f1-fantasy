namespace F1Fantasy.Api.Models
{
    public class RaceResultDriver
    {
        public int RaceResultId { get; set; }
        public RaceResult RaceResult { get; set; } = null!;

        public int DriverId { get; set; }
        public Driver Driver { get; set; } = null!;

        public int ConstructorId { get; set; }
        public Constructor Constructor { get; set; } = null!;

        public int Position { get; set; }
        public bool DidFinish { get; set; }
        public bool FastestLap { get; set; }
    }
}
