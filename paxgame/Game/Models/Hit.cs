using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Models
{
    public record Hit
    {
        public BattleUnit Unit { get; init; }
        public double Damage { get; init; }
        public int Attacs { get; init; } // 0 = spell damage
        public int Delay { get; set; }

        public Hit(BattleUnit unit, double damage, int attacs, int delay = 0)
        {
            Unit = unit;
            Damage = damage;
            Attacs = attacs;
            Delay = delay;
        }
    }
}
