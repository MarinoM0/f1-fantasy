namespace F1Fantasy.Api.DTOs
{
    public class CreateFantasyTeamRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public List<int> ConstructorIds { get; set; } = new();
        public List<int> DriverIds { get; set; } = new();
    }
}
