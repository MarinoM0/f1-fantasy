using System.ComponentModel.DataAnnotations;

namespace F1Fantasy.Api.DTOs.Predictions
{
    public class CreatePredictionRequestDto
    {
        [Range(1, int.MaxValue)]
        public int RaceId { get; set; }


        [Range(1, int.MaxValue)]
        public int P1DriverId { get; set; }

        [Range(1, int.MaxValue)]
        public int P2DriverId { get; set; }

        [Range(1, int.MaxValue)]
        public int P3DriverId { get; set; }
    }
}
