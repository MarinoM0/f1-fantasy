using System.ComponentModel.DataAnnotations;

namespace F1Fantasy.Api.DTOs.Leagues
{
    public class JoinLeagueRequestDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string InviteCode { get; set; } = string.Empty;
    }
}
