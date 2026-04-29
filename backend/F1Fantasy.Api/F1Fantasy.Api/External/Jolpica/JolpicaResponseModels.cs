using System.Text.Json.Serialization;

namespace F1Fantasy.Api.External.Jolpica
{
    public class JolpicaResponse
    {
        [JsonPropertyName("MRData")]
        public JolpicaMrData MRData { get; set; } = new();
    }

    public class JolpicaMrData
    {
        [JsonPropertyName("StandingsTable")]
        public JolpicaStandingsTable? StandingsTable { get; set; }

        [JsonPropertyName("RaceTable")]
        public JolpicaRaceTable? RaceTable { get; set; }
    }

    public class JolpicaStandingsTable
    {
        [JsonPropertyName("season")]
        public string Season { get; set; } = string.Empty;

        [JsonPropertyName("round")]
        public string Round { get; set; } = string.Empty;

        [JsonPropertyName("StandingsLists")]
        public List<JolpicaStandingsList> StandingsLists { get; set; } = [];
    }

    public class JolpicaStandingsList
    {
        [JsonPropertyName("season")]
        public string Season { get; set; } = string.Empty;

        [JsonPropertyName("round")]
        public string Round { get; set; } = string.Empty;

        [JsonPropertyName("DriverStandings")]
        public List<JolpicaDriverStandingItem> DriverStandings { get; set; } = [];

        [JsonPropertyName("ConstructorStandings")]
        public List<JolpicaConstructorStandingItem> ConstructorStandings { get; set; } = [];
    }

    public class JolpicaDriverStandingItem
    {
        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty;

        [JsonPropertyName("positionText")]
        public string PositionText { get; set; } = string.Empty;

        [JsonPropertyName("points")]
        public string Points { get; set; } = string.Empty;

        [JsonPropertyName("wins")]
        public string Wins { get; set; } = string.Empty;

        [JsonPropertyName("Driver")]
        public JolpicaDriver Driver { get; set; } = new();

        [JsonPropertyName("Constructors")]
        public List<JolpicaConstructorRef> Constructors { get; set; } = [];
    }

    public class JolpicaConstructorStandingItem
    {
        [JsonPropertyName("position")]
        public string Position { get; set; } = string.Empty;

        [JsonPropertyName("positionText")]
        public string PositionText { get; set; } = string.Empty;

        [JsonPropertyName("points")]
        public string Points { get; set; } = string.Empty;

        [JsonPropertyName("wins")]
        public string Wins { get; set; } = string.Empty;

        [JsonPropertyName("Constructor")]
        public JolpicaConstructorRef Constructor { get; set; } = new();
    }

    public class JolpicaDriver
    {
        [JsonPropertyName("driverId")]
        public string DriverId { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("givenName")]
        public string GivenName { get; set; } = string.Empty;

        [JsonPropertyName("familyName")]
        public string FamilyName { get; set; } = string.Empty;
    }

    public class JolpicaConstructorRef
    {
        [JsonPropertyName("constructorId")]
        public string ConstructorId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class JolpicaRaceTable
    {
        [JsonPropertyName("season")]
        public string Season { get; set; } = string.Empty;

        [JsonPropertyName("round")]
        public string Round { get; set; } = string.Empty;

        [JsonPropertyName("Races")]
        public List<JolpicaRace> Races { get; set; } = [];
    }

    public class JolpicaRace
    {
        [JsonPropertyName("season")]
        public string Season { get; set; } = string.Empty;

        [JsonPropertyName("round")]
        public string Round { get; set; } = string.Empty;

        [JsonPropertyName("raceName")]
        public string RaceName { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("Circuit")]
        public JolpicaCircuit Circuit { get; set; } = new();

        [JsonPropertyName("Results")]
        public List<JolpicaRaceResultItem> Results { get; set; } = [];
    }

    public class JolpicaCircuit
    {
        [JsonPropertyName("circuitId")]
        public string CircuitId { get; set; } = string.Empty;

        [JsonPropertyName("circuitName")]
        public string CircuitName { get; set; } = string.Empty;

        [JsonPropertyName("Location")]
        public JolpicaLocation Location { get; set; } = new();
    }

    public class JolpicaLocation
    {
        [JsonPropertyName("lat")]
        public string? Latitude { get; set; }

        [JsonPropertyName("long")]
        public string? Longitude { get; set; }

        [JsonPropertyName("locality")]
        public string Locality { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }

    public class JolpicaRaceResultItem
    {
        [JsonPropertyName("position")]
        public string? Position { get; set; }

        [JsonPropertyName("positionText")]
        public string PositionText { get; set; } = string.Empty;

        [JsonPropertyName("points")]
        public string Points { get; set; } = string.Empty;

        [JsonPropertyName("grid")]
        public string? Grid { get; set; }

        [JsonPropertyName("laps")]
        public string? Laps { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("Driver")]
        public JolpicaDriver Driver { get; set; } = new();

        [JsonPropertyName("Constructor")]
        public JolpicaConstructorRef Constructor { get; set; } = new();

        [JsonPropertyName("Time")]
        public JolpicaRaceTime? Time { get; set; }

        [JsonPropertyName("FastestLap")]
        public JolpicaFastestLap? FastestLap { get; set; }
    }

    public class JolpicaRaceTime
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }

    public class JolpicaFastestLap
    {
        [JsonPropertyName("rank")]
        public string? Rank { get; set; }

        [JsonPropertyName("Time")]
        public JolpicaFastestLapTime? Time { get; set; }
    }

    public class JolpicaFastestLapTime
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }
    }
}
