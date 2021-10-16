
using System.Collections.Concurrent;
using System.Numerics;

namespace Game.Models
{
    public class PaxGame
    {
        public const int buildX = 25;
        public const int buildY = 15;
        public const int buildXY = buildX * buildY;

        public const int TerranUnits = 3;
        public const int TerranUpgrades = 3;
        public const int ZergUnits = 4;
        public const int ZergUpgrades = 3;

        public const int boardX = 1000;
        public const int boardY = 600;

        public int Round { get; set; } = 1;
        public int Step { get; set; } = 128;
        public float StepMs { get; set; } = 25;
        public int Gameloop { get; set; } = 0;
        public List<Player> Players { get; set; } = new List<Player>();
        public int X = 1000;
        public int Y = 600;
        public int BX = 15 * 20;
        public int BY = 25 * 20;
        public BattleUnit Base1 { get; set; }
        public BattleUnit Base2 { get; set; }
        public List<BattleUnit> BattleUnits { get; set; } = new List<BattleUnit>();
        public List<BattleUnit> DisabledUnits { get; set; } = new List<BattleUnit>();
        public TeamData Team1 { get; set; } = new TeamData();
        public TeamData Team2 { get; set; } = new TeamData();
        public ConcurrentDictionary<Vector2, byte> Occupied { get; set; } = new ConcurrentDictionary<Vector2, byte>();
        public double ArmyValueKilledTeam1 { get; set; }
        public double ArmyValueKilledTeam2 { get; set; }
        public bool GenStyle { get; set; } = false;

        public PaxGame()
        {
            Base1 = new BattleUnit(Program.GameUnits.Base, new HashSet<Ability>(), 0, new Vector2(0 + 4, Y / 2), 1, X);
            Base2 = new BattleUnit(Program.GameUnits.Base, new HashSet<Ability>(), 1, new Vector2(X - 4, Y / 2), 1, X);
        }

        public void Init()
        {
            Gameloop = 0;
            BattleUnits = new List<BattleUnit>();
            int id = 10;
            foreach (var player in Players)
            {
                foreach (var unit in player.Units)
                {
                    foreach (var pos in unit.Value)
                    {
                        id++;
                        BattleUnits.Add(new BattleUnit(unit.Key, player.Abilities, id, pos, player.Team, X));
                    }
                }
            }
            DisabledUnits = new List<BattleUnit>();
            Occupied = new ConcurrentDictionary<Vector2, byte>();
            //Base1 = new BattleUnit(Program.GameUnits.Base, 0, new Vector2(0 + 4, Y / 2), 1, X);
            //Base2 = new BattleUnit(Program.GameUnits.Base, 1, new Vector2(X - 4, Y / 2), 1, X);
        }

        public bool isOccupied(Vector2 pos)
        {
            if (Occupied.ContainsKey(pos) || OutOfBounds(pos))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void removeOldOccupied(Vector2 oldpos, int unitSize)
		{
            int size = unitSize * 10 + 10;
            for (int i = 0; i < size; i++)
            {
                Occupied.TryRemove(new Vector2(oldpos.X - size / 2 + i, oldpos.Y - size / 2 + i), out _);
            }
        }

        public void setOccupied(Vector2 newpos, int unitSize)
        {
            int size = unitSize * 10 + 10;
            for (int i = 0; i < size; i++)
            {
                Occupied.AddOrUpdate(new Vector2(newpos.X - size / 2 + i, newpos.Y - size / 2 + i), 0, (x, y) => y = 0);
            }
        }

        public bool OutOfBounds(Vector2 pos)
        {
            if (pos.X < 0 || pos.X > X)
                return true;
            if (pos.Y < 0 || pos.Y > Y)
                return true;
            return false;
        }
    }
}