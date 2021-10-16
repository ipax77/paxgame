using paxgame.Shared;
using System.Net.Http.Json;

namespace paxgame.app.Client.Services
{
    public class PlayerService
    {
        private readonly HttpClient http;
        private readonly ILogger<PlayerService> logger;

        public string Race { get; set; } = "Terran";
        public string OppRace { get; set; } = "Zerg";
        public int Minerals { get; set; }
        public List<UnitResult>? AvailabelUnits { get; set; }
        public UnitResult? SelectedUnit { get; set; }
        public List<PlayerUnit> Units { get; set; }
        public List<UnitResult>? AvailabelOpponentUnits { get; set; }
        public List<PlayerUnit> OpponentUnits { get; set; }
        public HashSet<int> Moves { get; set; }
        public HashSet<int> OpponentMoves { get; set; }

        public PlayerService(HttpClient Http, ILogger<PlayerService> logger)
        {
            http = Http;
            this.logger = logger;
            Units = new List<PlayerUnit>();
            OpponentUnits = new List<PlayerUnit>();
            Moves = new HashSet<int>();
            OpponentMoves = new HashSet<int>();
        }

        public async Task GetUnits()
        {
            if (AvailabelUnits == null || !AvailabelUnits.Any())
            {
                try
                {
                    AvailabelUnits = await http.GetFromJsonAsync<List<UnitResult>>($"api/v1/paxgame/units/{Race}");
                    AvailabelOpponentUnits = await http.GetFromJsonAsync<List<UnitResult>>($"api/v1/paxgame/units/{OppRace}");
                } catch (Exception e)
                {
                    logger.LogError($"failed getting units: {e.Message}");
                    AvailabelUnits = new List<UnitResult>();
                }
            }
        }
    }

    public record PlayerUnit
    {
        public UnitResult Unit { get; init; }
        public myVector2 ScreenPos { get; init; }
        public myVector2 BuildPos { get; init; }
        public bool Fixed { get; set; } = false;

        public PlayerUnit(UnitResult unit, myVector2 buildPos, myVector2 screenPos)
        {
            Unit = unit;
            BuildPos = buildPos;
            ScreenPos = screenPos;
        }
    }
}
