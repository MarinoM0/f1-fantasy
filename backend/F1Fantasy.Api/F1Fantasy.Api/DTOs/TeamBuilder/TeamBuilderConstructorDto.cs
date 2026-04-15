namespace F1Fantasy.Api.DTOs.TeamBuilder
{
    public class TeamBuilderConstructorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public int? StandingPosition { get; set; }
        public decimal CurrentPoints { get; set; }
        public int CurrentWins { get; set; }
        public bool HasLiveData { get; set; }
    }
}
