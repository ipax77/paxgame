using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace paxgame.Shared
{
    public record GameResult
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Loop {  get; init; }
        public int Step {  get; init; }
        public TimeSpan StepTimespan {  get; init; }
        public List<UnitResult>? Units { get; init; }
        public string? Style { get; set; }
        public double ArmyValueKilledTeam1 { get; init; }
        public double ArmyValueKilledTeam2 { get; init; }
        public List<int>[] PlayerMoves { get; init; }

        public GameResult()
        {
            PlayerMoves = new List<int>[2] { new List<int>(), new List<int>() };
        }
    }


    public record UnitResult
    {
        public int Id { get; init; }
        public int UnitId { get; init; }
        public string Name { get; init; } = String.Empty;
        public string ImageName { get; init; } = String.Empty;
        public int Size { get; init; }
        public int Team { get; init; }
        public int Spawn { get; init; }
        public int Gameloop {  get; init; }
        public myVector2? BuildPosition {  get; init; }
        public List<StyleInfo> Path { get; set; } = new List<StyleInfo>();
        public List<AbilityInfo> AbilityPath {  get; init; } = new List<AbilityInfo>();
        double distance { get; set; }
        public int Cost { get; set; }

        public myVector2? GetPosition(int gameloop)
        {
            if (!Path.Any())
                return null;
            else if (Gameloop < gameloop)
                return null;
            else return Path.Last(f => f.Gameloop <= gameloop).Pos;
        }

        public double GetOpacity(int gameloop, int step)
        {
            if (gameloop >= Gameloop)
                return 0;
            if (gameloop == Gameloop - step)
                return 0.25;
            if ((gameloop == Gameloop - step * 2))
                return 0.5;
            if ((gameloop == Gameloop - step * 3))
                return 0.75;
            return 1;
        }

        public double GetOffset(int gameloop)
        {
            if (distance == 0)
            {
                distance = Path.Sum(s => s.Distance);
            }
            var dist = Path.Where(x => x.Gameloop <= gameloop).Sum(s => s.Distance);
            return Math.Round(dist * 100.0 / distance, 2);
        }
    }

    public sealed record myVector2
    {
        public float X { get; init; }
        public float Y { get; init; }
        [NotMapped]
        [JsonIgnore]
        public Vector2 Pos => new Vector2(X, Y);

        public myVector2() { }

        public myVector2(Vector2 pos) : this()
        {
            // X = MathF.Round(pos.X, 2);
            // Y = MathF.Round(pos.Y, 2);
            X = pos.X;
            Y = pos.Y;
        }

        public bool Equals(myVector2? other) => X == other?.X && Y == other?.Y;
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    public record StyleInfo
    {
        public int Gameloop { get; init; }
        public myVector2 Pos { get; init; }
        public float Distance { get; init; }
        public myVector2 Direction { get; init; }
        public double Speed { get; init; }

        public StyleInfo(int gameloop, myVector2 pos, float distance, myVector2 direction, double speed)
        {
            Gameloop = gameloop;
            Pos = pos;
            Distance = distance;
            Direction = direction;
            Speed = speed;

        }
    }

    public record AbilityInfo
    {
        public string Ability { get; init; } = String.Empty;
        public int Gameloop {  get; init; }
        public myVector2 Pos {  get; init; } = new myVector2();
    }

    public record DelayInfo
    {
        public int FromGameloop { get; init; }
        public int ToGameloop { get; init; }
        public double OffsetDistance { get; init; }
    }
}
