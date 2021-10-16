using Game.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Abilities
{
    public class EnergyAbility : UnitAbility
    {
        public EnergyAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            unit.BattleEnergy += (0.7875 / 30);
            if (unit.BattleEnergy > unit.Unit.MaxEnergy)
            {
                unit.BattleEnergy = unit.Unit.MaxEnergy;
            }
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
        }
    }
}
