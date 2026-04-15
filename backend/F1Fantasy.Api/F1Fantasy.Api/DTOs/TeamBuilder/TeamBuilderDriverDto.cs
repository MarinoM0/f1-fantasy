namespace F1Fantasy.Api.DTOs.TeamBuilder
{
    public class TeamBuilderDriverDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int ConstructorId { get; set; }
        public string ConstructorName { get; set; } = string.Empty;
        public string ConstructorCode { get; set; } = string.Empty;

        public int? StandingPosition { get; set; }
        public decimal CurrentPoints { get; set; }
        public int CurrentWins { get; set; }
        public bool HasLiveData { get; set; }
    }
}
