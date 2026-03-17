namespace F1Fantasy.Api.Models
{
    public class FantasyTeam : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal BudgetCap { get; set; } 
        public decimal RemainingBudget { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int ConstructorId { get; set; }
        public Constructor Constructor { get; set; } = null!;

        public ICollection<FantasyTeamDriver> FantasyTeamDrivers { get; set; } = new List<FantasyTeamDriver>();
    }
}
