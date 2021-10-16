
using Game.Models;
using System.Collections.Immutable;

namespace Game.Units
{
    public class GameUnits
    {
        public HashSet<Unit> Units;
        public Dictionary<AbilityName,Ability> Abilities;
        public Unit Base;
        public GameUnits()
        {
            Abilities = new Dictionary<AbilityName, Ability>()
            {
                { AbilityName.CombatShield, new Ability(ImmutableHashSet.Create(AbilityTrigger.Permanent))
                    {
                        AbilityName = AbilityName.CombatShield,
                        Cost = 50
                    }
                },
                { AbilityName.Stim, new Ability(ImmutableHashSet.Create(AbilityTrigger.Attac))
                    {
                        AbilityName = AbilityName.Stim,
                        Cost = 100,
                        Duration = TimeSpan.FromSeconds(11),
                        Cooldown = TimeSpan.Zero,
                        Visualize = true
                    }
                },
                { AbilityName.ConcussiveShells, new Ability(ImmutableHashSet.Create(AbilityTrigger.Hit))
                    {
                        AbilityName = AbilityName.ConcussiveShells,
                        Cost = 25
                    }
                },
                { AbilityName.ConcussiveShellsCast, new Ability(ImmutableHashSet.Create(AbilityTrigger.Act))
                    {
                        AbilityName = AbilityName.ConcussiveShellsCast,
                        Duration = TimeSpan.FromSeconds(1.07),
                    }
                },
                { AbilityName.KD8Charge, new Ability(ImmutableHashSet.Create(AbilityTrigger.Attac))
                    {
                        AbilityName = AbilityName.KD8Charge,
                        Cooldown = TimeSpan.FromSeconds(14),
                        Range = 5,
                        Radius = 2,
                        Visualize = true
                    }
                },
                { AbilityName.KD8ChargeCast, new Ability(ImmutableHashSet.Create(AbilityTrigger.Act))
                    {
                        AbilityName = AbilityName.KD8ChargeCast,
                        Cooldown = TimeSpan.FromSeconds(0.2),
                        Radius = 2
                    }
                },
                {
                    AbilityName.MetabolicBoost, new Ability(ImmutableHashSet.Create(AbilityTrigger.EnemyInSight))
                    {
                        AbilityName = AbilityName.MetabolicBoost,
                        Cost = 100
                    }
                },
                {
                    AbilityName.VolatileBurst, new Ability(ImmutableHashSet.Create(AbilityTrigger.Attac, AbilityTrigger.Death))
                    {
                        AbilityName = AbilityName.VolatileBurst,
                        Range = 2.2,
                        Visualize = true
                    }
                },
                {
                    AbilityName.CentrifugalHooks, new Ability(ImmutableHashSet.Create(AbilityTrigger.EnemyInSight))
                    {
                        AbilityName = AbilityName.CentrifugalHooks,
                        Cost = 125
                    }
                },
                {
                    AbilityName.GlialReconstitution, new Ability(ImmutableHashSet.Create(AbilityTrigger.EnemyInSight))
                    {
                        AbilityName = AbilityName.GlialReconstitution,
                        Cost = 100
                    }
                },
                {
                    AbilityName.Transfusion, new Ability(ImmutableHashSet.Create(AbilityTrigger.FriendInRange))
                    {
                        AbilityName = AbilityName.Transfusion,
                        Range = 7,
                        EnergyCost = 50,
                        Visualize = true,
                        Cooldown = TimeSpan.FromSeconds(1)
                    }
                },
                {
                    AbilityName.TransfusionCast, new Ability(ImmutableHashSet.Create(AbilityTrigger.Act))
                    {
                        AbilityName = AbilityName.TransfusionCast,
                        Duration = TimeSpan.FromSeconds(7)
                    }
                },
                {
                    AbilityName.Energy, new Ability(ImmutableHashSet.Create(AbilityTrigger.Act))
                    {
                        AbilityName = AbilityName.Energy,
                    }
                },
            };


            Units = new HashSet<Unit>();
            
            var marine = new Unit(
                "Marine",
                1,
                9,
                3.15,
                50,
                new List<UnitAttribute>() { UnitAttribute.Biological, UnitAttribute.Light },
                new Attac()
                {
                    Damage = 6,
                    Modifier = 1,
                    Range = 5,
                    Attacs = 1,
                    Cooldown = 0.61,
                    Dps = 9.8
                },
                new Defence()
                {
                    Healthpoints = 45,
                    Armor = 0,
                    Modifier = 1
                },
                new List<Ability>()
                {
                    Abilities[AbilityName.CombatShield],
                    Abilities[AbilityName.Stim]
                }
            )
            { Id = 1,
              Race = Race.Terran
            };
            Units.Add(marine);

            var marauder = new Unit(
                "Marauder",
                2,
                10,
                3.15,
                125,
                new List<UnitAttribute>() { UnitAttribute.Biological, UnitAttribute.Armored },
                new Attac()
                {
                    Damage = 10,
                    Modifier = 1,
                    Range = 6,
                    Attacs = 1,
                    Cooldown = 1.07,
                    Dps = 14.1,
                    BonusDamages = new List<BonusDamage>() { new BonusDamage()
                    {
                        Attribute = UnitAttribute.Armored,
                        Damage = 10,
                        Modifier = 1
                    } }
                },
                new Defence()
                {
                    Healthpoints = 125,
                    Armor = 1,
                    Modifier = 1
                },
                new List<Ability>()
                {
                    Abilities[AbilityName.ConcussiveShells],
                    Abilities[AbilityName.Stim]
                }
            )
            { Id = 2, Race = Race.Terran };
            Units.Add(marauder);

            var reaper = new Unit(
                "Reaper",
                1,
                9,
                5.25,
                75,
                new List<UnitAttribute>() { UnitAttribute.Biological, UnitAttribute.Light },
                new Attac()
                {
                    Damage = 4,
                    Modifier = 1,
                    Range = 5,
                    Attacs = 2,
                    Cooldown = 0.79,
                    Dps = 10.1
                },
                new Defence()
                {
                    Healthpoints = 60,
                    Armor = 0,
                    Modifier = 1
                },
                new List<Ability>()
                {
                    Abilities[AbilityName.KD8Charge]
                }
            )
            { Id = 3, Race = Race.Terran };
            Units.Add(reaper);

            var zergling = new Unit(
                "Zergling",
                1,
                8,
                // 0,
                4.13,
                25,
                new List<UnitAttribute>() { UnitAttribute.Biological },
                new Attac()
                {
                    Damage = 5,
                    Modifier = 1,
                    Range = 0.1,
                    Attacs = 1,
                    Cooldown = 0.497,
                    Dps = 10
                },
                new Defence()
                {
                    Healthpoints = 35,
                    Armor = 0
                },
                new List<Ability>()
                {
                    Abilities[AbilityName.MetabolicBoost]
                }
            )
            { Id = 1, Race = Race.Zerg };
            Units.Add(zergling);

            var baneling = new Unit(
                "Baneling",
                1,
                8,
                // 0,
                3.5,
                50,
                new List<UnitAttribute>() { UnitAttribute.Biological },
                new Attac()
                {
                    Damage = 0,
                    Modifier = 2,
                    Range = 0.1,
                    Attacs = 0,
                    Cooldown = 0,
                    Dps = 0
                },
                new Defence()
                {
                    Healthpoints = 30,
                    Armor = 0
                },
                new List<Ability>()
                {
                    Abilities[AbilityName.CentrifugalHooks],
                    Abilities[AbilityName.VolatileBurst]
                }
            )
            { Id = 2, Race = Race.Zerg };
            Units.Add(baneling);

            var roach = new Unit(
                "Roach",
                2,
                9,
                // 0,
                3.15,
                80,
                new List<UnitAttribute>() { UnitAttribute.Biological },
                new Attac()
                {
                    Damage = 16,
                    Modifier = 2,
                    Range = 4,
                    Attacs = 1,
                    Cooldown = 1.43,
                    Dps = 11.2
                },
                new Defence()
                {
                    Healthpoints = 145,
                    Armor = 1,
                    Modifier = 1
                },
                new List<Ability>()
                {
                    Abilities[AbilityName.GlialReconstitution]
                }
            )
            { Id = 3, Race = Race.Zerg };
            Units.Add(roach);

            var queen = new Unit(
                "Queen",
                3,
                9,
                // 0,
                1.31,
                160,
                new List<UnitAttribute>() { UnitAttribute.Biological, UnitAttribute.Psionic },
                new Attac()
                {
                    Damage = 9,
                    Modifier = 1,
                    Range = 5,
                    Attacs = 2,
                    Cooldown = 0.71,
                    Dps = 11.2
                },
                new Defence()
                {
                    Healthpoints = 175,
                    Armor = 1,
                    Modifier = 1
                },
                new List<Ability>()
                {
                    Abilities[AbilityName.Transfusion],
                    Abilities[AbilityName.Energy]
                }
            )
            {
                Id = 4,
                Race = Race.Zerg,
                MaxEnergy = 200,
                StartingEnergy = 40
            };
            Units.Add(queen);

            var cc = new Unit(
                "Base",
                4,
                0,
                0,
                0,
                new List<UnitAttribute>() { UnitAttribute.Armored, UnitAttribute.Defence },
                new Attac()
                {
                    Damage = 11,
                    Modifier = 0,
                    Range = 11,
                    Attacs = 1,
                    Cooldown = 0.5,
                    Dps = 22
                },
                new Defence()
                {
                    Healthpoints = 1000,
                    Armor = 2
                },
                new List<Ability>()
                {

                }
            );
            Base = cc;
        }
    }
}
