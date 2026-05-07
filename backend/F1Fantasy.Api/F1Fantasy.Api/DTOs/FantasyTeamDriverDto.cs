namespace F1Fantasy.Api.DTOs
{
    public class FantasyTeamDriverDto
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

        public decimal PointsAtTransfer { get; set; }
    }
}
