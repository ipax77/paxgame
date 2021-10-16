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
    public class KD8ChargeAbility : UnitAbility
    {
        public KD8ChargeAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            var range = Ability.Range * 30 + 20; // TODO
            range *= 2;

            if (target != null)
            {
                var distances = new List<UnitDistance>();
                var enemies = unit.Team == 1 ? game.Team2.Units : game.Team1.Units;
                target.Distances = new List<UnitDistance>();
                var kd8Pos = GameService.MoveTowards(unit.Position, target.Position, Vector2.Distance(unit.Position, target.Position) * 0.9f);
                for (int i = 0; i < enemies.Count; i++)
                {
                    distances.Add(new UnitDistance(enemies[i], Vector2.DistanceSquared(kd8Pos, enemies[i].Position)));
                }

                foreach (var distance in distances.Where(x => MathF.Sqrt(x.Distance) <= range + x.Unit.Unit.Size * 5))
                {
                    var cast = new KD8ChargeCastAbility(Program.GameUnits.Abilities[AbilityName.KD8ChargeCast], kd8Pos, unit);
                    cast.SetCooldown();
                    distance.Unit.BattleAbilities.TryAdd(AbilityName.KD8ChargeCast, cast);
                }

                // base.Activate with different pos
                Active = true;
                SetDuration();
                SetCooldown();
                if (game.GenStyle && Ability.Visualize)
                {
                    unit.AbilityPath.Add(new AbilityInfo()
                    {
                        Ability = Enum.GetName(Ability.AbilityName) ?? "none",
                        Gameloop = unit.LastGameloop,
                        Pos = new myVector2(kd8Pos)
                    });
                }
            }
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
        }
    }
}
