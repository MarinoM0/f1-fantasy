namespace F1Fantasy.Api.DTOs
{
    public class ConstructorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }

    }
}
