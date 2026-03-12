namespace F1Fantasy.Api.Models
{
    public class Driver : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int ConstructorId { get; set; }
        public Constructor Constructor { get; set; } = null;

        public ICollection<FantasyTeamDriver> FantasyTeamDrivers { get; set; } = new List<FantasyTeamDriver>();
        public ICollection<RaceResultDriver> RaceResultDrivers { get; set; } = new List<RaceResultDriver>();
    }
}
