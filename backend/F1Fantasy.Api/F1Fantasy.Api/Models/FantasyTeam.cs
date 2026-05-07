namespace F1Fantasy.Api.Models
{
    public class FantasyTeam : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal BudgetCap { get; set; }
        public decimal RemainingBudget { get; set; }

        public bool HasUsedTransfer { get; set; } = false;
        public decimal LockedInPoints { get; set; } = 0;

        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public ICollection<FantasyTeamDriver> FantasyTeamDrivers { get; set; } = new List<FantasyTeamDriver>();
        public ICollection<FantasyTeamConstructor> FantasyTeamConstructors { get; set; } = new List<FantasyTeamConstructor>();
    }
}