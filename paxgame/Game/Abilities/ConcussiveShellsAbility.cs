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
    public class ConcussiveShellsAbility : UnitAbility
    {
        public ConcussiveShellsAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            if (target != null)
            {
                target.BattleAbilities.AddOrUpdate(AbilityName.ConcussiveShellsCast, new ConcussiveShellsCastAbility(Program.GameUnits.Abilities[AbilityName.ConcussiveShellsCast]), (k, v) => { v.SetDuration(); return v; });
            }
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
        }
    }
}
