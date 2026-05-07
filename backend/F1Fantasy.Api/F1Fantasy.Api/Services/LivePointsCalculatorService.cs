using F1Fantasy.Api.DTOs.Jolpica;
using F1Fantasy.Api.Models;

namespace F1Fantasy.Api.Services
{

    public static class LivePointsCalculatorService
    {
 
        public static IReadOnlyDictionary<string, decimal> BuildDriverPointsByCode(
            IReadOnlyList<JolpicaDriverStandingDto> driverStandings)
        {
            var driverPointsByCode = new Dictionary<string, decimal>();

            foreach (var standing in driverStandings)
            {
                if (string.IsNullOrWhiteSpace(standing.Code))
                {
                    continue;
                }

                var key = NormalizeKey(standing.Code);

                if (driverPointsByCode.ContainsKey(key))
                {
                    continue;
                }

                driverPointsByCode[key] = standing.Points;
            }

            return driverPointsByCode;
        }

        public static IReadOnlyDictionary<string, decimal> BuildConstructorPointsByJolpicaId(
            IReadOnlyList<JolpicaConstructorStandingDto> constructorStandings)
        {
            var constructorPointsByJolpicaId = new Dictionary<string, decimal>();

            foreach (var standing in constructorStandings)
            {
                if (string.IsNullOrWhiteSpace(standing.ConstructorId))
                {
                    continue;
                }

                var key = NormalizeKey(standing.ConstructorId);

                if (constructorPointsByJolpicaId.ContainsKey(key))
                {
                    continue;
                }

                constructorPointsByJolpicaId[key] = standing.Points;
            }

            return constructorPointsByJolpicaId;
        }

        public static decimal GetDriverPoints(
            Driver driver,
            IReadOnlyDictionary<string, decimal> driverPointsByCode)
        {
            var key = NormalizeKey(driver.Code);

            if (driverPointsByCode.TryGetValue(key, out var points))
            {
                return points;
            }

            return 0m;
        }

        public static decimal GetConstructorPoints(
            Constructor constructor,
            IReadOnlyDictionary<string, decimal> constructorPointsByJolpicaId)
        {
            if (string.IsNullOrWhiteSpace(constructor.JolpicaConstructorId))
            {
                return 0m;
            }

            var key = NormalizeKey(constructor.JolpicaConstructorId);

            if (constructorPointsByJolpicaId.TryGetValue(key, out var points))
            {
                return points;
            }

            return 0m;
        }


        public static decimal CalculateTeamPoints(
            FantasyTeam team,
            IReadOnlyDictionary<string, decimal> driverPointsByCode,
            IReadOnlyDictionary<string, decimal> constructorPointsByJolpicaId)
        {
            var driverContribution = 0m;

            foreach (var teamDriver in team.FantasyTeamDrivers)
            {
                var currentPoints = GetDriverPoints(teamDriver.Driver, driverPointsByCode);
                var pointsSinceJoining = currentPoints - teamDriver.PointsAtTransfer;

                if (pointsSinceJoining < 0m)
                {
                    pointsSinceJoining = 0m;
                }

                driverContribution += pointsSinceJoining;
            }

            var constructorContribution = 0m;

            foreach (var teamConstructor in team.FantasyTeamConstructors)
            {
                var currentPoints = GetConstructorPoints(
                    teamConstructor.Constructor, constructorPointsByJolpicaId);
                var pointsSinceJoining = currentPoints - teamConstructor.PointsAtTransfer;

                if (pointsSinceJoining < 0m)
                {
                    pointsSinceJoining = 0m;
                }

                constructorContribution += pointsSinceJoining;
            }

            return team.LockedInPoints + driverContribution + constructorContribution;
        }

        private static string NormalizeKey(string value)
        {
            return value.Trim().ToUpperInvariant();
        }
    }
}
