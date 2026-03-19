namespace F1Fantasy.Api.DTOs
{
    public class FantasyTeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BudgetCap { get; set; }
        public decimal RemainingBudget { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        public int ConstructorId { get; set; }
        public string ConstructorName { get; set; } = string.Empty;
        public string ConstructorCode { get; set; } = string.Empty;
        public decimal ConstructorPrice { get; set; }

        public List<FantasyTeamDriverDto> Drivers { get; set; } = new();
    }
}
