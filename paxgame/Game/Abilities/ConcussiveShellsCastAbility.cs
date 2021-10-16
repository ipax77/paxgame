using Game.Models;
using Game.Services;
using Game.Units;
using Microsoft.Extensions.Logging;
using paxgame.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game.Abilities
{
    public class ConcussiveShellsCastAbility : UnitAbility
    {
        public ConcussiveShellsCastAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            unit.BattleSpeed /= 2;
            base.Activate(unit, target, game);
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            unit.BattleSpeed *= 2;
            unit.BattleAbilities.TryRemove(Ability.AbilityName, out _);
        }
    }
}
