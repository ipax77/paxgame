using Game.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game.Abilities
{
    public class VolatileBurstAbility : UnitAbility
    {
        public VolatileBurstAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            var range = Ability.Range * 30 + 20; // TODO
            // range *= range;
                
            if (unit.Distances == null || !unit.Distances.Any())
            {
                var enemies = unit.Team == 1 ? game.Team2.Units : game.Team1.Units;
                unit.Distances = new List<UnitDistance>();
                for (int i = 0; i < enemies.Count; i++)
                {
                    unit.Distances.Add(new UnitDistance(enemies[i], Vector2.DistanceSquared(unit.Position, enemies[i].Position)));
                }
            }
            // var distances = unit.Distances.Select(f => new UnitDistance(f.Unit, MathF.Sqrt(f.Distance)));
            // foreach (var distance in distances.Where(x => x.Distance <= range + x.Unit.Unit.Size * 5))
            foreach (var distance in unit.Distances.Where(x => MathF.Sqrt(x.Distance) <= range + x.Unit.Unit.Size * 5))
            {
                var damage = 16 + (unit.BattleAttac.Upgrades * unit.BattleAttac.Attac.Modifier);
                if (distance.Unit.Unit.Attributes.Contains(UnitAttribute.Light))
                {
                    damage += 19 + (unit.BattleAttac.Upgrades * unit.BattleAttac.Attac.Modifier);
                }
                distance.Unit.Hits.Add(new Hit(unit, damage, 1));
            }
            unit.BattleDefence.BattleHealthpoints = 0;
            base.Activate(unit, target, game);
            this.Cooldown = 99;
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
        }
    }
}
