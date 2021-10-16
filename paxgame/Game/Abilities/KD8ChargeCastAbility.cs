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
    public class KD8ChargeCastAbility : UnitAbility
    {
        public KD8ChargeCastAbility(Ability ability, Vector2 kd8pos, BattleUnit unit) : base(ability)
        {
            KD8Pos = kd8pos;
            Caster = unit;
        }

        private Vector2 KD8Pos;
        private BattleUnit Caster;

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            var range = Ability.Radius * 30 + 20; // TODO
            var distance = Vector2.Distance(KD8Pos, unit.Position);
            if (distance <= range)
            {
                unit.Hits.Add(new Hit(Caster, 5, 1, 0));
                unit.TargetUnit = null;
                unit.Position = GameService.MoveTowards(unit.Position, KD8Pos, distance / -2);
            }
            unit.BattleAbilities.TryRemove(Ability.AbilityName, out _);
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
        }
    }
}
