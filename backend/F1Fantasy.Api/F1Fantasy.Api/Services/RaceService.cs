using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class RaceService
    {
        private readonly AppDbContext _dbContext;

        public RaceService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<RaceDto>> GetAllAsync()
        {
            return await _dbContext.Races
                .AsNoTracking()
                .OrderBy(r => r.RoundNumber)
                .Select(r => new RaceDto
                {
                    Id = r.Id,
                    RoundNumber = r.RoundNumber,
                    Name = r.Name,
                    CircuitName = r.CircuitName,
                    Country = r.Country,
                    StartTimeUtc = r.StartTimeUtc,
                    IsCompleted = r.IsCompleted
                })
                .ToListAsync();
        }
    }
}
