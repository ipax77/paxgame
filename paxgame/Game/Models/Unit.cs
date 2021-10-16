
using System.Collections.Immutable;

namespace Game.Models
{
    public record Unit
    {
        public int Id { get; init; }
        public Race Race {  get; init; }
        public string Name { get; init; }
        public byte Size { get; init; }
        public double Sight { get; init; }
        public double Speed { get; init; }
        public double Cost { get; init; }
        public List<UnitAttribute> Attributes { get; init; }
        public Attac AttacGround { get; init; }
        public Defence Defence { get; init; }
        public List<Ability> Abilities { get; init; }
        public double MaxEnergy {  get; init; }
        public double StartingEnergy { get; init; }

        // public Unit() { }

        public Unit(string name, byte size, double sight, double speed, double cost, List<UnitAttribute> attributes,  Attac attac, Defence defance, List<Ability> abilities)
        {
            Name = name;
            Size = size;
            Sight = sight;
            Speed = speed;
            Cost = cost;
            Attributes = attributes;
            AttacGround = attac;
            Defence = defance;
            Abilities = abilities;
        }
    }

    public record Attac
    {
        public double Damage { get; init; }
        public int Attacs { get; init; }
        public double Dps { get; init; }
        public double Cooldown { get; init; }
        public double Range { get; init; }
        public double Modifier { get; init; }
        public List<BonusDamage> BonusDamages {  get; init; } = new List<BonusDamage>();
    }

    public record BonusDamage
    {
        public UnitAttribute Attribute { get; init; }
        public double Damage { get; init; }
        public double Modifier { get; init; }

    }

    public record Defence
    {
        public double Healthpoints { get; init; }
        public double Armor { get; init; }
        public double Modifier { get; init; }
    }

    public sealed record Ability
    {
        public AbilityName AbilityName { get; init; }
        public ImmutableHashSet<AbilityTrigger> Trigger { get; init; }
        public double Range { get; init; } = 0;
        public double Radius { get; init; } = 0;
        public double EnergyCost { get; init; } = 0;
        public int Cost { get; init; } = 0;
        public bool Visualize { get; init; } = false;
        public TimeSpan Cooldown { get; init; } = TimeSpan.Zero;
        public TimeSpan Duration { get; init; } = TimeSpan.Zero;

        public bool Equals(Ability? other) => this.AbilityName == other?.AbilityName;

        public override int GetHashCode()
        {
            // return base.GetHashCode();
            return (int)this.AbilityName;
        }

        public Ability(ImmutableHashSet<AbilityTrigger> trigger)
        {
            Trigger = trigger;
        }
    }

    public enum Race
    {
        Terran,
        Protoss,
        Zerg
    }

    public enum UnitAttribute
    {
        Armored,
        Light,
        Psionic,
        Biological,
        Defence
    }

    public enum AbilityName
    {
        None = 0,
        Stim = 1,
        CombatShield = 2,
        ConcussiveShells = 3,
        ConcussiveShellsCast = 4,
        KD8Charge = 6,
        KD8ChargeCast = 7,
        MetabolicBoost = 8,
        CentrifugalHooks = 9,
        VolatileBurst = 10,
        GlialReconstitution = 11,
        Transfusion = 12,
        TransfusionCast = 13,
        Energy = 14,

    }

    public enum UnitAbilitiy
    {
        Hp,
        AttacSpeed,
        MoveSpeed,
        Splash,
        Granade,
        Slow,
        Energy,
    }

    public enum AbilityTrigger
    {
        Permanent,
        Act,
        EnemyInSight,
        EnemyInRange,
        FriendInRange,
        Attac,
        Hit,
        Death
    }
}