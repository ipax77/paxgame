using Game.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Abilities
{
    public class CombatShieldAbility : UnitAbility
    {
        public CombatShieldAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            unit.BattleDefence.BattleHealthpoints += 10;
            unit.ImageName = "pax_marine_shield";
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
        }
    }
}
