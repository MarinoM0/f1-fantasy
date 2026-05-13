using System.ComponentModel.DataAnnotations;

namespace F1Fantasy.Api.DTOs.Leagues
{
    public class CreateLeagueRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }
}
