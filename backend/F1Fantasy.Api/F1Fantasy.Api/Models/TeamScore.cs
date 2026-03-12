namespace F1Fantasy.Api.Models;

public class TeamScore : BaseEntity
{
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public int RaceId { get; set; }
    public Race Race { get; set; } = null!;

    public int Points { get; set; }
}