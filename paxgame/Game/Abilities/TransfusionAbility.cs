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
    public class TransfusionAbility : UnitAbility
    {
        public TransfusionAbility(Ability ability) : base(ability)
        {
        }

        protected override void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            if (unit.BattleEnergy >= 50)
            {
                var targets = unit.Team == 1 ? game.Team1.GetHealableUnits() : game.Team2.GetHealableUnits();
                if (targets.Any())
                {
                    var range = Ability.Range * 30 + 20; // TODO
                    range *= 2;

                    var distances = new List<UnitDistance>();
                    for (int i = 0; i < targets.Count; i++)
                    {
                        distances.Add(new UnitDistance(targets[i], Vector2.DistanceSquared(unit.Position, targets[i].Position)));
                    }

                    foreach (var distance in distances.Where(x => MathF.Sqrt(x.Distance) <= range + x.Unit.Unit.Size * 5).OrderBy(o => o.Distance))
                    {
                        if (distance.Unit.Id == unit.Id)
                        {
                            continue;
                        }

                        if (distance.Unit.BattleDefence.BattleHealthpoints < distance.Unit.Unit.Defence.Healthpoints - 100)
                        {
                            if (unit.Team == 1)
                            {
                                game.Team1.RemoveHealableUnit(distance.Unit);
                            }
                            else
                            {
                                game.Team2.RemoveHealableUnit(distance.Unit);
                            }
                        }

                        distance.Unit.BattleAbilities.AddOrUpdate(AbilityName.TransfusionCast, new TransfusionCastAbility(Program.GameUnits.Abilities[AbilityName.TransfusionCast]), (k, v) => { v.SetTimer(); return v; });

                        if (unit.Team == 1)
                        {
                            lock (game.Team1.lockobject)
                            {
                                if (distance.Unit.BattleDefence.BattleHealthpoints <= distance.Unit.Unit.Defence.Healthpoints - 50)
                                {
                                    distance.Unit.BattleDefence.BattleHealthpoints += 50;
                                } else
                                {
                                    game.Team1.RemoveHealableUnit(distance.Unit);
                                    continue;
                                }
                            }
                        } else
                        {
                            lock (game.Team2.lockobject)
                            {
                                if (distance.Unit.BattleDefence.BattleHealthpoints <= distance.Unit.Unit.Defence.Healthpoints - 50)
                                {
                                    distance.Unit.BattleDefence.BattleHealthpoints += 50;
                                }
                                else
                                {
                                    game.Team2.RemoveHealableUnit(distance.Unit);
                                    continue;
                                }
                            }
                        }
                        unit.BattleEnergy -= 50;
                        base.Activate(unit, target, game);
                        Program.logger.LogDebug($"casting transfusion from {unit.Id} to {distance.Unit.Id}");
                        return;
                    }
                }
            }
        }

        protected override void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
        }
    }
}
