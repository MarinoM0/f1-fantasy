using F1Fantasy.Api.Data;
using F1Fantasy.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace F1Fantasy.Api.Services
{
    public class DriverService
    {
        private readonly AppDbContext _dbContext;

        public DriverService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<DriverDto>> GetAllAsync()
        {
            return await _dbContext.Drivers
                .AsNoTracking()
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .Select(d => new DriverDto
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = d.FirstName + " " + d.LastName,
                    Code = d.Code,
                    Price = d.Price,
                    ConstructorId = d.ConstructorId,
                    ConstructorName = d.Constructor.Name,
                    ConstructorCode = d.Constructor.Code
                })
                .ToListAsync();
        }
    }
}
