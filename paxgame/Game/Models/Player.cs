
using System.Numerics;
namespace Game.Models
{
    public class Player
    {
        public int Team { get; set; }
        public int Minerals { get; set; } = 500;
        public int Tier { get; set; } = 1;
        public List<Unit> AvailableUnits { get; set; } = new List<Unit>();
        public Dictionary<Unit, List<Vector2>> Units { get; set; } = new Dictionary<Unit, List<Vector2>>();
        public HashSet<Ability> AvailableAbilities { get; set; } = new HashSet<Ability>();
        public HashSet<Ability> Abilities {  get; set; } = new HashSet<Ability>();
        public HashSet<int> Moves {  get; set; } = new HashSet<int>();
        public double ArmyValue => !Units.Any() ? 0 : Units.Sum(s => s.Key.Cost * s.Value.Count);
    }
}