namespace F1Fantasy.Api.DTOs
{
    public class CreateFantasyTeamRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public int ConstructorId { get; set; }
        public List<int> DriverIds { get; set; } = new();
    }
}
