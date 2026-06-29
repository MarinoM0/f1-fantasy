namespace F1Fantasy.Api.Models
{
    // A timestamped price snapshot written each time prices are recalculated.
    // Exactly one of DriverId / ConstructorId is set. Lets us chart price trends
    // later without recomputing history.
    public class PriceHistory : BaseEntity
    {
        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }

        public int? ConstructorId { get; set; }
        public Constructor? Constructor { get; set; }

        public decimal Price { get; set; }
    }
}
