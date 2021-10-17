
using Game.Services;
using System.Numerics;
namespace Game.Models
{
    public class Player
    {
        public int Team { get; set; }
        public int Minerals { get; set; }
        public int Tier { get; set; } = 1;
        public Race Race { get; set; } = Race.Terran;
        public List<Unit> AvailableUnits { get; set; } = new List<Unit>();
        public Dictionary<Unit, HashSet<Vector2>> Units { get; set; } = new Dictionary<Unit, HashSet<Vector2>>();
        public HashSet<Ability> AvailableAbilities { get; set; } = new HashSet<Ability>();
        public HashSet<Ability> Abilities { get; set; } = new HashSet<Ability>();
        public HashSet<int> Moves { get; set; } = new HashSet<int>();
        public double ArmyValue => !Units.Any() ? 0 : Units.Sum(s => s.Key.Cost * s.Value.Count);
        private HashSet<Vector2> Occupied = new HashSet<Vector2>();

        public void AddMoves(int[] moves)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                AddMove(moves[i]);
            }
        }

        public Unit? AddMove(int move)
        {
            (int unitId, Vector2 pos) = GetUnitPos(move);
            Unit unit = Program.GameUnits.Units.First(f => f.Race == Race && f.Id == unitId);
            Vector2 boardPos = new Vector2(pos.Y * 20f, pos.X * 20f);
            Vector2 boardPosV = Team == 1 ?
                  GameService.mirrorImage(0, -1, PaxGame.boardY / 2, boardPos)
                : GameService.mirrorImage(-1, 0, PaxGame.boardX / 2, boardPos);

            // center build area
            boardPosV.Y = Team == 1 ? boardPosV.Y - 50 : boardPosV.Y + 50;

            if (Occupied.Contains(boardPosV))
            {
                return null;
            } else
            {
                Occupied.Add(boardPosV);
            }

            if (!Units.ContainsKey(unit))
            {
                Units[unit] = new HashSet<Vector2>() { boardPosV };
            }
            else
            {
                Units[unit].Add(boardPosV);
            }
            Minerals += (int)unit.Cost;
            Moves.Add(move);
            return unit;
        }

        public Unit? AddRandomMove()
        {
            int units = Race switch
            {
                Race.Terran => PaxGame.TerranUnits,
                Race.Zerg => PaxGame.ZergUnits,
                _ => PaxGame.TerranUnits
            };
            int move = random_except_list((PaxGame.buildX - 1) * (PaxGame.buildY - 1) * units, GetExceptList());
            return AddMove(move);
        }

        private (int, Vector2) GetUnitPos(int move)
        {
            int pos, unit = Math.DivRem(move, PaxGame.buildXY, out pos);
            return (unit + 1, GetPos(pos));
        }

        private Vector2 GetPos(int pos)
        {
            int uy, ux = Math.DivRem(pos, PaxGame.buildY, out uy);
            return new Vector2(ux, uy);
        }
        
        private int random_except_list(int n, HashSet<int> x)
        {
            var range = Enumerable.Range(0, n).Where(i => !x.Contains(i));
            int index = Program.random.Next(0, n - x.Count);
            return range.ElementAt(index);
        }

        private HashSet<int> GetExceptList()
        {
            int units = Race switch
            {
                Race.Terran => PaxGame.TerranUnits,
                Race.Zerg => PaxGame.ZergUnits,
                _ => PaxGame.TerranUnits
            };

            HashSet<int> occupied = new HashSet<int>();
            foreach (var move in Moves)
            {
                int pos, unit = Math.DivRem(move, PaxGame.buildXY, out pos);
                for (int i = 0; i < units; i++)
                {
                    occupied.Add(PaxGame.buildXY * i + pos);
                }
            }
            return occupied;
        }

        public Unit? UndoLastMove()
        {
            if (!Moves.Any())
            {
                return null;
            }
            int move = Moves.Last();
            (int unitId, Vector2 pos) = GetUnitPos(move);
            Unit unit = Program.GameUnits.Units.First(f => f.Race == Race && f.Id == unitId);
            Vector2 boardPos = new Vector2(pos.Y * 20f, pos.X * 20f);
            Vector2 boardPosV = Team == 1 ?
                  GameService.mirrorImage(0, -1, PaxGame.boardY / 2, boardPos)
                : GameService.mirrorImage(-1, 0, PaxGame.boardX / 2, boardPos);
            boardPosV.Y = Team == 1 ? boardPosV.Y - 50 : boardPosV.Y + 50;
            Units[unit].Remove(boardPosV);
            Minerals -= (int)unit.Cost;
            return unit;
        }
    }
}