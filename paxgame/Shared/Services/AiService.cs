using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace paxgame.Shared.Services
{
    public static class AiService
    {
        public const int buildX = 25;
        public const int buildY = 15;
        public const int buildXY = buildX * buildY;
        
        public const int TerranUnits = 3;
        public const int TerranUpgrades = 3;
        public const int ZergUnits = 4;
        public const int ZergUpgrades = 3;

        public const int boardX = 50;
        public const int boardY = 30;

        public static int GetUnitMove(int unitId, myVector2 unitPos)
        {
            int move = 0;
            for (int x = 0; x < buildX; x++)
            {
                for (int y = 0; y < buildY; y++)
                {
                    if (x == unitPos.X && y == unitPos.Y)
                    {
                        return move + ((unitId - 1) * buildXY);
                    }
                    move++;
                }
            }
            return -1;
        }

        public static int GetUpgradeMove(int upgradeId, string race)
        {
            return race switch {
                "Terran" => TerranUnits * buildXY + upgradeId,
                "Zerg" => ZergUnits * buildXY + upgradeId,
                _ => TerranUnits * buildXY + upgradeId,
            };
        }

        public static (int, myVector2) GetUnitPos(int move)
        {
            int pos, unit = Math.DivRem(move, buildXY, out pos);
            return (unit + 1, GetPos(pos));
        }

        private static myVector2 GetPos(int pos)
        {
            int uy, ux = Math.DivRem(pos, buildY, out uy);
            return new myVector2 { X = ux, Y = uy };
        }

        public static HashSet<int> GetExceptList(HashSet<int> moves, string race)
        {
            int units = race switch
            {
                "Terran" => TerranUnits,
                "Zerg" => ZergUnits,
                _ => TerranUnits
            };

            HashSet<int> occupied = new HashSet<int>();
            foreach (var move in moves)
            {
                int pos, unit = Math.DivRem(move, buildXY, out pos);
                for (int i = 0; i < units; i++)
                {
                    occupied.Add(buildXY * i + pos);
                }
            }
            return occupied;
        }
    }
}
