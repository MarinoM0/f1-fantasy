using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class ConstructorService
    {
        private readonly AppDbContext _dbContext;

        public ConstructorService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task <IReadOnlyList<ConstructorDto>> GetAllAsync()
        {
            return await _dbContext.Constructors
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new ConstructorDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Price = c.Price
                })
                .ToListAsync();
        }
    }
}
