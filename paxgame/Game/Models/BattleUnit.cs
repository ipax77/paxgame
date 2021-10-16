
using System.Collections.Concurrent;
using System.Numerics;
using Game.Abilities;
using Game.Services;
using Microsoft.Extensions.Logging;
using paxgame.Shared;

namespace Game.Models
{
    public class BattleUnit
    {
        public int Id { get; set; }
        public int Team { get; set; }
        public Unit Unit { get; set; }
        public BattleUnit? TargetUnit { get; set; }
        public BattleDefence BattleDefence { get; set; }
        public BattleAttac BattleAttac { get; set; }
        public ConcurrentDictionary<AbilityName, UnitAbility> BattleAbilities { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 BuildPosition { get; set; }
        public Vector2 MainTarget { get; set; }
        public List<StyleInfo> Path { get; set; }
        public List<AbilityInfo> AbilityPath { get; set; }
        public UnitSats Stats { get; set; }
        public bool TargetPosReached { get; set; } = false;
        public int SpawnGameloop { get; set; }
        public int LastGameloop { get; set; }
        public ConcurrentBag<Hit> Hits { get; set; }
        public int SwitchTarget { get; set; }
        public double BattleSpeed { get; set; }
        public double BattleEnergy { get; set; }
        public List<UnitDistance>? Distances { get; set; }
        public string ImageName { get; set; }

        // public BattleUnit() { }

        public BattleUnit(Unit unit, HashSet<Ability> abilities, int id, Vector2 buildPosition, int team, int gameX)
        {
            Unit = unit;
            Id = id;
            BattleDefence = new BattleDefence(unit.Defence);
            BattleAttac = new BattleAttac(unit.AttacGround);
            Position = buildPosition;
            BuildPosition = buildPosition;
            MainTarget = GameService.mirrorImage(-1, 0, gameX / 2, BuildPosition);
            Path = new List<StyleInfo>();
            Team = team;
            Stats = new UnitSats();
            Hits = new ConcurrentBag<Hit>();
            BattleSpeed = unit.Speed;
            BattleAbilities = UnitAbility.GetAbilities(unit, abilities);
            BattleEnergy = unit.StartingEnergy;
            AbilityPath = new List<AbilityInfo>();
            ImageName = "pax_" + unit.Name;
        }

        public UnitResult ViewModel()
        {
            return new UnitResult()
            {
                Id = Id,
                UnitId = Unit.Id,
                Name = Unit.Name,
                ImageName = ImageName,
                Size = Unit.Size,
                Team = Team,
                Spawn = SpawnGameloop,
                Gameloop = LastGameloop,
                BuildPosition = new myVector2(BuildPosition),
                Path = Path,
                AbilityPath = AbilityPath,
                Cost = (int)Unit.Cost
            };
        }

        public double GetSight()
        {
            return this.Unit.Sight * 30;
        }

        public double GetActRange()
        {
            return this.BattleAttac.GetRange() * 30;
        }

        public double GetSpeed(PaxGame game, Vector2 target)
        {
            // return this.Unit.Speed;
            if (target == Vector2.Zero)
            {
                if (this.Unit.Speed == 0)
                    return 0;
                else
                    return 2;
            }
            else
                return this.BattleSpeed;
        }

        public void Act(PaxGame game)
        {
            if (SpawnGameloop == 0)
                SpawnGameloop = game.Gameloop;

            LastGameloop = game.Gameloop;

            if (this.BattleDefence.BattleHealthpoints <= 0)
            {
                return;
            }

            // BattleAbilities.Where(x => x.Ability.Trigger.Contains(AbilityTrigger.Act)).ToList().ForEach(f => f.Trigger(this, this.TargetUnit, game));
            for (int i = BattleAbilities.Count - 1; i >= 0; i--)
            {
                if (BattleAbilities.ElementAt(i).Value.Ability.Trigger.Contains(AbilityTrigger.Act) || BattleAbilities.ElementAt(i).Value.Ability.Trigger.Contains(AbilityTrigger.FriendInRange))
                {
                    BattleAbilities.ElementAt(i).Value.Trigger(this, this.TargetUnit, game);
                }
            }

            //if (game.Gameloop < 1024)
            //{
            //    Move(game, Vector2.Zero, 0);
            //    return;
            //}

            var enemies = Team == 1 ? game.Team2.Units : game.Team1.Units;

            double distance = 0;

            if (TargetUnit == null || SwitchTarget > 0 || TargetUnit.BattleDefence.BattleHealthpoints <= 0)
            {
                Distances = new List<UnitDistance>();
                for (int i = 0; i < enemies.Count; i++)
                {
                    Distances.Add(new UnitDistance(enemies[i], Vector2.DistanceSquared(this.Position, enemies[i].Position)));
                }
                Distances = Distances.OrderBy(o => o.Distance).ToList();

                var firstDistance = Distances.First();
                if (SwitchTarget > 0 && Distances.Count > SwitchTarget)
                {
                    firstDistance = Distances[SwitchTarget];
                }
                TargetUnit = firstDistance.Unit;
                distance = MathF.Sqrt(firstDistance.Distance);
            }
            else
            {
                distance = Vector2.Distance(this.Position, TargetUnit.Position);
            }

            if (distance <= GetActRange() + TargetUnit.Unit.Size * 5) // +1 = melee mod
            {
                Program.logger.LogDebug($"{game.Gameloop} {Id} Interacting with {TargetUnit.Id} ({distance})");
                Interact(TargetUnit, game);

            }
            else if (distance <= GetSight())
            {
                // TODO: what if other unit came in attac range?
                Program.logger.LogDebug($"{game.Gameloop} {Id} has Sight to {TargetUnit.Id} ({distance})");
                Move(game, TargetUnit.Position, distance);
            }
            else
            {
                TargetUnit = null;
                Move(game, Vector2.Zero, distance);
            }
        }

        public void Move(PaxGame game, Vector2 target, double distance)
        {
            var speed = GetSpeed(game, target);

            if (speed <= 0)
                return;

            if (target == Vector2.Zero)
            {
                if (TargetPosReached)
                {
                    target = Team == 1 ? game.Base2.BuildPosition : game.Base1.BuildPosition;
                }
                else
                {
                    target = MainTarget;
                }
            }
            else
            {
                distance -= GetActRange();
            }

            //Vector2 direction = Vector2.Normalize(target - Position);

            //var move = direction * (float)speed;
            //var newpos = new Vector2(Position.X + move.X, Position.Y + move.Y);


            var newpos = GameService.MoveTowards(Position, target, (float)Math.Min(distance, speed));

            //if (distance > 0)
            //{
            //    var newdistance = Vector2.Distance(newpos, target);
            //    if (GetActRange() > newdistance)
            //    {
            //        move = direction * (float)GetActRange();
            //        newpos = new Vector2(target.X - move.X, target.Y - move.Y);
            //        Program.logger.LogDebug($"{game.Gameloop} {Id} Throttling speed from {distance} to {newdistance} ({GameService.Distance(newpos, target)})");
            //    }
            //}

            //if ((Position.X < target.X && newpos.X > target.X) || Position.X > target.X && newpos.X < target.X)
            //    newpos.X = target.X;
            //if ((Position.Y < target.Y && newpos.Y > target.Y) || Position.Y > target.Y && newpos.Y < target.Y)
            //    newpos.Y = target.Y;

            if (newpos == MainTarget)
            {
                TargetPosReached = true;
            }
            var newIntpos = GetIntPos(newpos);
            int i = 0;

            game.removeOldOccupied(GetIntPos(Position), Unit.Size);

            while (game.isOccupied(newIntpos))
            {
                if (i > 2)
                {
                    SwitchTarget++;
                    if (SwitchTarget < 10)
                    {
                        Act(game);
                        return;
                    }
                }
                var rotation = GameService.GetRandomDirection();
                //var newdirection = GameService.RotatePoint(direction, Vector2.Zero, rotation);
                //var move = newdirection * (float)speed;
                //newpos = new Vector2(Position.X + move.X, Position.Y + move.Y);
                var newdirection = GameService.RotatePoint(target, Position, rotation);
                newpos = GameService.MoveTowards(Position, newdirection, (float)Math.Min(distance, speed));
                newIntpos = GetIntPos(newpos);
                Program.logger.LogDebug($"{game.Gameloop} {Id} Rotating {rotation} from {Position} to {newpos}");
                i++;
                if (i > 6)
                {
                    Program.logger.LogInformation($"{game.Gameloop} {Id} Unit stuck at {newpos}");
                    game.setOccupied(Position, Unit.Size);
                    return;
                }
            }

            game.setOccupied(newIntpos, Unit.Size);

            Program.logger.LogDebug($"{game.Gameloop} {Id} Moving from {Position} to {newpos}");

            if (game.GenStyle)
            {
                Path.Add(new StyleInfo(game.Gameloop, new myVector2(Position), Vector2.Distance(Position, newpos), new myVector2(Vector2.Normalize(newpos - Position)), speed));
            }

            Position = newpos;

            SwitchTarget = 0;
        }

        public static Vector2 GetIntPos(Vector2 pos)
        {
            return new Vector2(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y));
            // return new Vector2(MathF.Round(pos.X, 1), MathF.Round(pos.Y, 1));
        }

        public void Interact(BattleUnit unit, PaxGame game)
        {
            Attac(unit, game);
        }

        public void Attac(BattleUnit unit, PaxGame game)
        {
            BattleAbilities.Where(x => x.Value.Ability.Trigger.Contains(AbilityTrigger.Attac)).ToList().ForEach(x => x.Value.Trigger(this, unit, game));
            if (this.BattleAttac.Cooldown <= 0)
            {
                double damage = this.BattleAttac.Damage;
                for (int i = 0; i < this.BattleAttac.Attac.BonusDamages.Count; i++)
                {
                    if (unit.Unit.Attributes.Contains(this.BattleAttac.Attac.BonusDamages[i].Attribute))
                    {
                        damage += this.BattleAttac.Attac.BonusDamages[i].Damage;
                    }
                }

                unit.Hits.Add(new Hit(this, damage, this.BattleAttac.Attac.Attacs, 0));
                this.BattleAttac.Cooldown = Convert.ToInt32(this.BattleAttac.BattleCooldown * 30.0);
            } else
            {
                this.BattleAttac.Cooldown--;
            }
        }
    }

    public class BattleAttac
    {
        public Attac Attac { get; private set; }
        public double Damage { get; set; }
        public double Dps { get; set; }
        public double Range { get; set; }
        public double BattleCooldown { get; set; }
        public int Cooldown { get; set; }
        public int Upgrades { get; set; }

        public double GetRange()
        {
            return Attac.Range;
        }

        public BattleAttac(Attac attac)
        {
            Attac = attac;
            Damage = Attac.Damage;
            Dps = Attac.Dps;
            Range = Attac.Range;
            Cooldown = 0;
            BattleCooldown = attac.Cooldown;
        }
    }

    public class BattleDefence
    {
        public Defence Defence { get; private set; }
        public double BattleHealthpoints { get; set; }
        public double Armor { get; set; }

        public BattleDefence(Defence defence)
        {
            Defence = defence;
            BattleHealthpoints = defence.Healthpoints;
            Armor = defence.Armor;
        }
    }

    public class UnitSats
    {
        public double DamageDone { get; set; }
        public double DamageTaken { get; set; }
        public int Kills { get; set; }
    }

    public record UnitDistance
    {
        public BattleUnit Unit { get; init; }
        public float Distance { get; init; }
        public UnitDistance(BattleUnit unit, float distance)
        {
            Unit = unit;
            Distance = distance;
        }
    }
}