using Game.Models;
using Microsoft.Extensions.Logging;

namespace Game.Services
{
    public class PlayService
    {
        public static async Task PlayAsync(PaxGame game, ILogger logger, bool shuffle = false, int threads = 8)
        {
            game.Init();
            SemaphoreSlim sem = new SemaphoreSlim(threads, threads);

            for (int i = 0; i < game.BattleUnits.Count; i++)
            {
                foreach (var ab in game.BattleUnits[i].BattleAbilities.Where(x => x.Value.Ability.Trigger.Contains(AbilityTrigger.Permanent)))
                {
                    ab.Value.Trigger(game.BattleUnits[i], null, game);
                }
            }

            while (game.BattleUnits.Any())
            {
                game.Team1.Set(game.BattleUnits.Where(x => x.Team == 1).ToList());
                game.Team2.Set(game.BattleUnits.Where(x => x.Team == 2).ToList());

                if (!game.GenStyle && (!game.Team1.Units.Any() || !game.Team2.Units.Any()))
                {
                    Program.logger.LogDebug($"Breaking game at {game.Gameloop}");
                    break;
                }

                game.Team1.Units.Add(game.Base1);
                game.Team2.Units.Add(game.Base2);

                if (shuffle)
                {
                    game.BattleUnits.Shuffle();
                }

                EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
                int unitsDone = 0;

                foreach (var unit in game.BattleUnits)
                {
                    await sem.WaitAsync();
                    var task = Task.Factory.StartNew(() =>
                    {
                        unit.Act(game);
                        for (int i = unit.BattleAbilities.Count - 1; i >= 0; i--)
                        {
                            unit.BattleAbilities.ElementAt(i).Value.Tick(unit, unit.TargetUnit, game);
                        }
                    }).ContinueWith((ant) =>
                    {
                        sem.Release();
                        Interlocked.Increment(ref unitsDone);
                        if (unitsDone == game.BattleUnits.Count)
                        {
                            ewh.Set();
                        }

                    });
                }
                ewh.WaitOne();

                ApplyHits(game);

                game.Gameloop += game.Step;

                if (game.Gameloop > 200000)
                {
                    Program.logger.LogDebug($"Breaking game at maxLoop");
                    break;
                }
            }
            game.Round++;
        }

        public static void ApplyHits(PaxGame game)
        {
            foreach (var unit in game.BattleUnits.ToArray())
            {
                List<Hit> delayedHits = new List<Hit>();
                
                BattleUnit? killer = null;
                Hit? hit;
                unit.Hits.TryTake(out hit);
                    
                while (unit.BattleDefence.BattleHealthpoints > 0 && hit != null)
                {
                    if (hit.Delay > 0)
                    {
                        hit.Delay--;
                        delayedHits.Add(hit);
                    }
                    else
                    {
                        hit.Unit.BattleAbilities.Where(x => x.Value.Ability.Trigger.Contains(AbilityTrigger.Hit)).ToList().ForEach(f => f.Value.Trigger(hit.Unit, unit, game));
                        killer = GameService.Hit(hit, unit);
                    }
                    unit.Hits.TryTake(out hit);
                }

                if (unit.BattleDefence.BattleHealthpoints <= 0)
                {
                    if (killer != null && killer.Unit.Name != "Base")
                    {
                        if (unit.Team == 1)
                            game.ArmyValueKilledTeam2 += unit.Unit.Cost;
                        else
                            game.ArmyValueKilledTeam1 += unit.Unit.Cost;
                    }
                    unit.BattleAbilities.Where(x => x.Value.Ability.Trigger.Contains(AbilityTrigger.Death)).ToList().ForEach(x => x.Value.Trigger(unit, unit.TargetUnit, game));
                    game.DisabledUnits.Add(unit);
                    game.BattleUnits.Remove(unit);
                    game.removeOldOccupied(unit.Position, unit.Unit.Size);
                }
                else if (delayedHits.Any())
                {
                    for (int i = 0; i < delayedHits.Count; i++)
                        unit.Hits.Add(delayedHits[i]);
                }
            }
        }

        public static void Play(PaxGame game, bool shuffle = false)
        {
            game.Init();

            while (game.BattleUnits.Any())
            {
                game.Team1.Set(game.BattleUnits.Where(x => x.Team == 1).ToList());
                game.Team2.Set(game.BattleUnits.Where(x => x.Team == 2).ToList());
                game.Team1.Units.Add(game.Base1);
                game.Team2.Units.Add(game.Base2);

                if (shuffle)
                    game.BattleUnits.Shuffle();

                foreach (var unit in game.BattleUnits.ToArray())
                {
                    unit.Act(game);
                    for (int i = unit.BattleAbilities.Count - 1; i >= 0; i--)
                    {
                        unit.BattleAbilities.ElementAt(i).Value.Tick(unit, unit.TargetUnit, game);
                    }
                }

                ApplyHits(game);

                game.Gameloop += game.Step;

                if (game.Gameloop > 200000)
                {
                    Program.logger.LogDebug($"Breaking game at maxLoop");
                    break;
                }
            }
            game.Round++;
        }

    }
}