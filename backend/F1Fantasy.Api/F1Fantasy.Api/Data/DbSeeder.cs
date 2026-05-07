using F1Fantasy.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;

namespace F1Fantasy.Api.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext dbContext)
        {
            await SeedConstructorsAsync(dbContext);
            await SeedDriversAsync(dbContext);
            await SeedRacesAsync(dbContext);
        }

        private static async Task SeedConstructorsAsync(AppDbContext dbContext)
        {
            if (await dbContext.Constructors.AnyAsync())
            {
                return;
            }

            var constructors = new List<Constructor>
            {
                new() { Name = "McLaren",           Code = "MCL", Price = 28.50m, JolpicaConstructorId = "mclaren" },
                new() { Name = "Mercedes",          Code = "MER", Price = 29.90m, JolpicaConstructorId = "mercedes" },
                new() { Name = "Red Bull Racing",   Code = "RBR", Price = 28.80m, JolpicaConstructorId = "red_bull" },
                new() { Name = "Ferrari",           Code = "FER", Price = 23.90m, JolpicaConstructorId = "ferrari" },
                new() { Name = "Williams",          Code = "WIL", Price = 13.20m, JolpicaConstructorId = "williams" },
                new() { Name = "Racing Bulls",      Code = "RBT", Price = 7.50m,  JolpicaConstructorId = "rb" },
                new() { Name = "Aston Martin",      Code = "AMR", Price = 9.10m,  JolpicaConstructorId = "aston_martin" },
                new() { Name = "Haas F1 Team",      Code = "HAA", Price = 8.60m,  JolpicaConstructorId = "haas" },
                new() { Name = "Audi",              Code = "AUD", Price = 5.40m,  JolpicaConstructorId = "audi" },
                new() { Name = "Alpine",            Code = "ALP", Price = 13.70m, JolpicaConstructorId = "alpine" },
                new() { Name = "Cadillac",          Code = "CAD", Price = 5.20m,  JolpicaConstructorId = "cadillac" }
            };

            await dbContext.Constructors.AddRangeAsync(constructors);
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedDriversAsync(AppDbContext dbContext)
        {
            if (await dbContext.Drivers.AnyAsync())
            {
                return;
            }

            var constructors = await dbContext.Constructors.ToListAsync();

            var mcl = constructors.First(x => x.Code == "MCL");
            var mer = constructors.First(x => x.Code == "MER");
            var rbr = constructors.First(x => x.Code == "RBR");
            var fer = constructors.First(x => x.Code == "FER");
            var wil = constructors.First(x => x.Code == "WIL");
            var rbt = constructors.First(x => x.Code == "RBT");
            var amr = constructors.First(x => x.Code == "AMR");
            var haa = constructors.First(x => x.Code == "HAA");
            var aud = constructors.First(x => x.Code == "AUD");
            var alp = constructors.First(x => x.Code == "ALP");
            var cad = constructors.First(x => x.Code == "CAD");

            var drivers = new List<Driver>
            {
                new() { FirstName = "Lando", LastName = "Norris", Code = "NOR", Price = 26.80m, ConstructorId = mcl.Id },
                new() { FirstName = "Oscar", LastName = "Piastri", Code = "PIA", Price = 24.90m, ConstructorId = mcl.Id },

                new() { FirstName = "George", LastName = "Russell", Code = "RUS", Price = 28.00m, ConstructorId = mer.Id },
                new() { FirstName = "Kimi", LastName = "Antonelli", Code = "ANT", Price = 23.80m, ConstructorId = mer.Id },

                new() { FirstName = "Max", LastName = "Verstappen", Code = "VER", Price = 28.10m, ConstructorId = rbr.Id },
                new() { FirstName = "Isack", LastName = "Hadjar", Code = "HAD", Price = 13.90m, ConstructorId = rbr.Id },

                new() { FirstName = "Charles", LastName = "Leclerc", Code = "LEC", Price = 23.40m, ConstructorId = fer.Id },
                new() { FirstName = "Lewis", LastName = "Hamilton", Code = "HAM", Price = 22.90m, ConstructorId = fer.Id },

                new() { FirstName = "Carlos", LastName = "Sainz", Code = "SAI", Price = 12.20m, ConstructorId = wil.Id },
                new() { FirstName = "Alexander", LastName = "Albon", Code = "ALB", Price = 10.80m, ConstructorId = wil.Id },

                new() { FirstName = "Liam", LastName = "Lawson", Code = "LAW", Price = 6.9m, ConstructorId = rbt.Id },
                new() { FirstName = "Arvid", LastName = "Lindblad", Code = "LIN", Price = 7.40m, ConstructorId = rbt.Id },

                new() { FirstName = "Fernando", LastName = "Alonso", Code = "ALO", Price = 8.80m, ConstructorId = amr.Id },
                new() { FirstName = "Lance", LastName = "Stroll", Code = "STR", Price = 6.80m, ConstructorId = amr.Id },

                new() { FirstName = "Esteban", LastName = "Ocon", Code = "OCO", Price = 8.50m, ConstructorId = haa.Id },
                new() { FirstName = "Oliver", LastName = "Bearman", Code = "BEA", Price = 8.60m, ConstructorId = haa.Id },

                new() { FirstName = "Nico", LastName = "Hulkenberg", Code = "HUL", Price = 5.60m, ConstructorId = aud.Id },
                new() { FirstName = "Gabriel", LastName = "Bortoleto", Code = "BOR", Price = 6.40m, ConstructorId = aud.Id },

                new() { FirstName = "Pierre", LastName = "Gasly", Code = "GAS", Price = 12.80m, ConstructorId = alp.Id },
                new() { FirstName = "Franco", LastName = "Colapinto", Code = "COL", Price = 7.00m, ConstructorId = alp.Id },

                new() { FirstName = "Sergio", LastName = "Perez", Code = "PER", Price = 6.40m, ConstructorId = cad.Id },
                new() { FirstName = "Valtteri", LastName = "Bottas", Code = "BOT", Price = 4.70m, ConstructorId = cad.Id }
            };

            await dbContext.Drivers.AddRangeAsync(drivers);
            await dbContext.SaveChangesAsync();
        }

        private static async Task SeedRacesAsync(AppDbContext dbContext)
        {
            if (await dbContext.Races.AnyAsync())
            {
                return;
            }

            var races = new List<Race>
            {
                new() { RoundNumber = 1, Name = "Australian Grand Prix", CircuitName = "Albert Park", Country = "Australia", StartTimeUtc = new DateTime(2026, 3, 8, 4, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 2, Name = "Chinese Grand Prix", CircuitName = "Shanghai International Circuit", Country = "China", StartTimeUtc = new DateTime(2026, 3, 15, 7, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 3, Name = "Japanese Grand Prix", CircuitName = "Suzuka", Country = "Japan", StartTimeUtc = new DateTime(2026, 3, 29, 6, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 4, Name = "Bahrain Grand Prix", CircuitName = "Bahrain International Circuit", Country = "Bahrain", StartTimeUtc = new DateTime(2026, 4, 12, 15, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 5, Name = "Saudi Arabian Grand Prix", CircuitName = "Jeddah Corniche Circuit", Country = "Saudi Arabia", StartTimeUtc = new DateTime(2026, 4, 19, 17, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 6, Name = "Miami Grand Prix", CircuitName = "Miami International Autodrome", Country = "United States", StartTimeUtc = new DateTime(2026, 5, 3, 20, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 7, Name = "Canadian Grand Prix", CircuitName = "Circuit Gilles Villeneuve", Country = "Canada", StartTimeUtc = new DateTime(2026, 5, 24, 18, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 8, Name = "Monaco Grand Prix", CircuitName = "Circuit de Monaco", Country = "Monaco", StartTimeUtc = new DateTime(2026, 6, 7, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 9, Name = "Spanish Grand Prix", CircuitName = "Circuit de Barcelona-Catalunya", Country = "Spain", StartTimeUtc = new DateTime(2026, 6, 14, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 10, Name = "Austrian Grand Prix", CircuitName = "Red Bull Ring", Country = "Austria", StartTimeUtc = new DateTime(2026, 6, 28, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 11, Name = "British Grand Prix", CircuitName = "Silverstone", Country = "Great Britain", StartTimeUtc = new DateTime(2026, 7, 5, 14, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 12, Name = "Belgian Grand Prix", CircuitName = "Spa-Francorchamps", Country = "Belgium", StartTimeUtc = new DateTime(2026, 7, 19, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 13, Name = "Hungarian Grand Prix", CircuitName = "Hungaroring", Country = "Hungary", StartTimeUtc = new DateTime(2026, 7, 26, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 14, Name = "Dutch Grand Prix", CircuitName = "Zandvoort", Country = "Netherlands", StartTimeUtc = new DateTime(2026, 8, 23, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 15, Name = "Italian Grand Prix", CircuitName = "Monza", Country = "Italy", StartTimeUtc = new DateTime(2026, 9, 6, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 16, Name = "Madrid Grand Prix", CircuitName = "Madrid", Country = "Spain", StartTimeUtc = new DateTime(2026, 9, 13, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 17, Name = "Azerbaijan Grand Prix", CircuitName = "Baku City Circuit", Country = "Azerbaijan", StartTimeUtc = new DateTime(2026, 9, 27, 11, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 18, Name = "Singapore Grand Prix", CircuitName = "Marina Bay Street Circuit", Country = "Singapore", StartTimeUtc = new DateTime(2026, 10, 11, 12, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 19, Name = "United States Grand Prix", CircuitName = "Circuit of The Americas", Country = "United States", StartTimeUtc = new DateTime(2026, 10, 25, 19, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 20, Name = "Mexico City Grand Prix", CircuitName = "Autódromo Hermanos Rodríguez", Country = "Mexico", StartTimeUtc = new DateTime(2026, 11, 1, 20, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 21, Name = "São Paulo Grand Prix", CircuitName = "Interlagos", Country = "Brazil", StartTimeUtc = new DateTime(2026, 11, 8, 17, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 22, Name = "Las Vegas Grand Prix", CircuitName = "Las Vegas Strip Circuit", Country = "United States", StartTimeUtc = new DateTime(2026, 11, 22, 4, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 23, Name = "Qatar Grand Prix", CircuitName = "Lusail International Circuit", Country = "Qatar", StartTimeUtc = new DateTime(2026, 11, 29, 16, 0, 0, DateTimeKind.Utc), IsCompleted = false },
                new() { RoundNumber = 24, Name = "Abu Dhabi Grand Prix", CircuitName = "Yas Marina Circuit", Country = "United Arab Emirates", StartTimeUtc = new DateTime(2026, 12, 6, 13, 0, 0, DateTimeKind.Utc), IsCompleted = false }
            };

            await dbContext.Races.AddRangeAsync(races);
            await dbContext.SaveChangesAsync();
        }
    }
}
