using F1Fantasy.Api.DTOs.Predictions;

namespace F1Fantasy.Api.Interfaces
{
    public interface IPredictionService
    {
        Task<UpcomingPredictionDto> GetUpcomingAsync(int UserId, CancellationToken ct = default);
        Task<PredictionDto> SubmitAsync(int userId, CreatePredictionRequestDto request, CancellationToken ct = default);
        Task<IReadOnlyList<PredictionDto>> GetMyPredictionsAsync(int userId, CancellationToken ct = default);
        Task<IReadOnlyList<PredictionLeaderboardEntryDto>> GetLeaderboardAsync(CancellationToken ct = default);
    }
}
