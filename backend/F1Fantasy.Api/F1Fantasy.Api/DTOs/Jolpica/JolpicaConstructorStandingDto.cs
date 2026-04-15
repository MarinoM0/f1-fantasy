namespace F1Fantasy.Api.DTOs.Jolpica
{
    public class JolpicaConstructorStandingDto
    {
        public string Season { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;

        public int Position { get; set; }
        public string PositionText { get; set; } = string.Empty;
        public decimal Points { get; set; }
        public int Wins { get; set; }

        public string ConstructorId { get; set; } = string.Empty;
        public string ConstructorName { get; set; } = string.Empty;
    }
}
