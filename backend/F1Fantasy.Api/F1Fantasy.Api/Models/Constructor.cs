namespace F1Fantasy.Api.Models
{
    public class Constructor : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string JolpicaConstructorId { get; set; } = string.Empty;

        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
        public ICollection<FantasyTeam> FantasyTeams { get; set; } = new List<FantasyTeam>();
        public ICollection<RaceResultDriver> RaceResultDrivers { get; set; } = new List<RaceResultDriver>();
        public ICollection<FantasyTeamConstructor> FantasyTeamConstructors { get; set; } = new List<FantasyTeamConstructor>();
    }
}
