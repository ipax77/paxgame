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
    public class TransfusionCastAbility : UnitAbility
    {
        public TransfusionCastAbility(Ability ability) : base(ability)
        {
            SetTimer();
            step = 50 / (double)Timer;
        }

        private double step { get; set; }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            unit.BattleDefence.BattleHealthpoints += step;
            if (unit.BattleDefence.BattleHealthpoints > unit.Unit.Defence.Healthpoints)
            {
                unit.BattleDefence.BattleHealthpoints = unit.Unit.Defence.Healthpoints;
            }
            Timer--;
            if (Timer <= 0)
            {
                Deactivate(unit, target, game);
            }
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            unit.BattleAbilities.TryRemove(Ability.AbilityName, out _);
        }
    }
}
