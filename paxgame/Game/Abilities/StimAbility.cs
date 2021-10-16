using Game.Models;
using Microsoft.Extensions.Logging;
using paxgame.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Abilities
{
    public class StimAbility : UnitAbility
    {
        public StimAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            int minHp = unit.Unit.Id == 1 ? 10 : 20;
            if (unit.BattleDefence.BattleHealthpoints > minHp)
            {
                unit.BattleDefence.BattleHealthpoints -= minHp;
                if (unit.Unit.Id == 1)
                {
                    unit.BattleAttac.BattleCooldown -= 0.203;
                    unit.BattleSpeed += 1.57;
                } else // marauder
                {
                    unit.BattleAttac.BattleCooldown -= 0.36;
                    unit.BattleSpeed += 1.57;
                }
                base.Activate(unit, target, game);
            }
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            int minHp = unit.Unit.Id == 1 ? 10 : 20;
            if (unit.Unit.Id == 1)
            {
                unit.BattleAttac.BattleCooldown += 0.203;
                unit.BattleSpeed -= 1.57;
            }
            else // marauder
            {
                unit.BattleAttac.BattleCooldown += 0.36;
                unit.BattleSpeed -= 1.57;
            }
            base.Deactivate(unit, target, game);
        }
    }
}
