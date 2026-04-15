namespace F1Fantasy.Api.DTOs.Jolpica
{
    public class JolpicaDriverStandingDto
    {
        public string Season { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;

        public int Position { get; set; }
        public string PositionText { get; set; } = string.Empty;
        public decimal Points { get; set; }
        public int Wins { get; set; }

        public string DriverId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public string ConstructorId { get; set; } = string.Empty;
        public string ConstructorName { get; set; } = string.Empty;
    }
}
