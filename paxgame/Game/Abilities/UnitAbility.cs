using Game.Models;
using Microsoft.Extensions.Logging;
using paxgame.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Abilities
{
    public class UnitAbility
    {
        public static ConcurrentDictionary<AbilityName, UnitAbility> GetAbilities(Unit unit, HashSet<Ability> abilities)
        {
            var list = new ConcurrentDictionary<AbilityName, UnitAbility>();
            for (int i = 0; i < unit.Abilities.Count; i++)
            {
                if (unit.Abilities[i].Cost == 0 || abilities.Contains(unit.Abilities[i]))
                {
                    var bab = unit.Abilities[i].AbilityName switch
                    {
                        AbilityName.CombatShield => list.TryAdd(AbilityName.CombatShield, new CombatShieldAbility(unit.Abilities[i])),
                        AbilityName.Transfusion => list.TryAdd(AbilityName.Transfusion, new TransfusionAbility(unit.Abilities[i])),
                        AbilityName.ConcussiveShells => list.TryAdd(AbilityName.ConcussiveShells, new ConcussiveShellsAbility(unit.Abilities[i])),
                        AbilityName.KD8Charge => list.TryAdd(AbilityName.KD8Charge, new KD8ChargeAbility(unit.Abilities[i])),
                        AbilityName.Stim => list.TryAdd(AbilityName.Stim, new StimAbility(unit.Abilities[i])),
                        AbilityName.VolatileBurst => list.TryAdd(AbilityName.VolatileBurst, new VolatileBurstAbility(unit.Abilities[i])),
                        AbilityName.Energy => list.TryAdd(AbilityName.Energy, new EnergyAbility(unit.Abilities[i])),
                        _ => list.TryAdd(AbilityName.Energy, new EnergyAbility(unit.Abilities[i]))
                    };
                }
            }
            return list;
        }

        public Ability Ability { get; init; }

        public int Cooldown { get; set; }
        public int Duration { get; set; }
        public bool Active { get; set; } = false;
        public int Timer { get; set; }

        public UnitAbility(Ability ability)
        {
            Ability = ability;
        }

        public bool Tick(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            if (Cooldown > 0)
            {
                Cooldown--;
            }
            if (Duration > 0)
            {
                Duration--;
                if (Duration == 0)
                {
                    Deactivate(unit, target, game);
                    return true;
                }
            }
            return false;
        }

        public bool Trigger(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            if (Cooldown == 0 && Duration == 0)
            {
                Activate(unit, target, game);
                return true;
            } else
            {
                return false;
            }
        }

        protected virtual void Activate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            Active = true;
            SetDuration();
            SetCooldown();
            if (game.GenStyle && Ability.Visualize)
            {
                unit.AbilityPath.Add(new AbilityInfo()
                {
                    Ability = Enum.GetName(Ability.AbilityName) ?? "none",
                    Gameloop = unit.LastGameloop,
                    Pos = new myVector2(unit.Position)
                });
            }
        }

        public void SetDuration()
        {
            if (Ability.Duration != TimeSpan.Zero)
            {
                Duration = Convert.ToInt32(Ability.Duration.TotalSeconds * 300.0);
            }
        }

        public void SetCooldown()
        {
            if (Ability.Cooldown != TimeSpan.Zero)
            {
                Cooldown = Convert.ToInt32(Ability.Cooldown.TotalSeconds * 300.0);
            }
        }

        public void SetTimer()
        {
            Timer = Convert.ToInt32(Ability.Duration.TotalSeconds * 300.0);
        }

        protected virtual void Deactivate(BattleUnit unit, BattleUnit? target, PaxGame game)
        {
            Active = false;
        }
    }
}
