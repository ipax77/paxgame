using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Models;
using Game.Units;
using Microsoft.Extensions.Logging;
using Game.Services;
using System.Diagnostics;
using System.Text;

namespace Game
{
    public class Program
    {
        public static GameUnits GameUnits = new GameUnits();
        public static ILogger<Program> logger = ApplicationLogging.CreateLogger<Program>();
        public static Random random = new Random();


        public static void Main(string[] args)
        {
            
            
            for (int i = 0; i < 20; i++)
            {
                List<KeyValuePair<double, double>> results = new List<KeyValuePair<double, double>>();
                List<double> times = new List<double>();
                (int[][] moves, double[] result) = PlayRandomGameAsync(1000, new int[2][] { new int[0], new int[0] }).GetAwaiter().GetResult();
                logger.LogInformation($"player1 moves: {String.Join(",", moves[0])}");
                logger.LogInformation($"player2 moves: {String.Join(",", moves[1])}");
                Stopwatch sw = new Stopwatch();
                
                for (int j = 0; j < 100; j++)
                {
                    sw.Start();
                    (_, result) = PlayRandomGameAsync(1000, new int[2][] { moves[0], moves[1] }).GetAwaiter().GetResult();
                    sw.Stop();
                    results.Add(new KeyValuePair<double, double>(result[0], result[1]));
                    times.Add(sw.ElapsedMilliseconds);
                    sw.Reset();
                }
                logger.LogInformation($"AsyncResults {results.Select(s => s.Key).Average()}|{results.Select(s => s.Value).Average()} ({results.Select(s => s.Key).Min()}|{results.Select(s => s.Value).Min()}|{results.Select(s => s.Key).Max()}|{results.Select(s => s.Value).Max()}) - {Math.Round(times.Average(), 2)}ms");
                results.Clear();
                times.Clear();
                for (int j = 0; j < 10; j++)
                {
                    sw.Start();
                    (_, result) = PlayRandomGame(1000, new int[2][] { moves[0], moves[1] });
                    sw.Stop();
                    results.Add(new KeyValuePair<double, double>(result[0], result[1]));
                    times.Add(sw.ElapsedMilliseconds);
                    sw.Reset();
                }
                logger.LogInformation($"SyncResults {results.Select(s => s.Key).Average()}|{results.Select(s => s.Value).Average()} ({results.Select(s => s.Key).Min()}|{results.Select(s => s.Value).Min()}|{results.Select(s => s.Key).Max()}|{results.Select(s => s.Value).Max()}) - {Math.Round(times.Average(), 2)}ms");
            }



            Console.ReadLine();
        }

        public static (int[][], double[]) PlayRandomGame(int minerals, int[][] moves, bool shuffle = true)
        {
            Player player1 = new Player() { Team = 1, Race = Race.Terran };
            Player player2 = new Player() { Team = 2, Race = Race.Zerg };

            PaxGame game = new PaxGame();
            game.GenStyle = false;
            game.Players.Add(player1);
            game.Players.Add(player2);

            player1.AddMoves(moves[0]);
            player2.AddMoves(moves[1]);

            while (player1.Minerals < minerals)
            {
                player1.AddRandomMove();
            }
            if (player1.Minerals > minerals)
            {
                player1.UndoLastMove();
            }

            while (player2.Minerals < minerals)
            {
                player2.AddRandomMove();
            }
            if (player2.Minerals > player1.Minerals)
            {
                player2.UndoLastMove();
            }

            PlayService.Play(game, shuffle);

            return (new int[2][] { player1.Moves.ToArray(), player2.Moves.ToArray() }, new double[2] { game.ArmyValueKilledTeam1, game.ArmyValueKilledTeam2 });
        }

        public static async Task<(int[][], double[])> PlayRandomGameAsync(int minerals, int[][] moves, bool shuffle = true)
        {
            Player player1 = new Player() { Team = 1, Race = Race.Terran };
            Player player2 = new Player() { Team = 2, Race = Race.Zerg };

            PaxGame game = new PaxGame();
            game.GenStyle = false;
            game.Players.Add(player1);
            game.Players.Add(player2);

            if (moves[0].Any())
            {
                player1.AddMoves(moves[0]);
                player2.AddMoves(moves[1]);
            }
            else
            {
                while (player1.Minerals < minerals)
                {
                    player1.AddRandomMove();
                }
                if (player1.Minerals > minerals)
                {
                    player1.UndoLastMove();
                }

                while (player2.Minerals < minerals)
                {
                    player2.AddRandomMove();
                }
                if (player2.Minerals > player1.Minerals)
                {
                    player2.UndoLastMove();
                }
            }
            await PlayService.PlayAsync(game, logger, shuffle);

            return (new int[2][] { player1.Moves.ToArray(), player2.Moves.ToArray() }, new double[2] { game.ArmyValueKilledTeam1, game.ArmyValueKilledTeam2 });
        }

        public static void CleanUpPath()
        {

        }

        public static void AiHelper()
        {
            int length = 18;
            List<int> Rewards = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                var state = Enumerable.Repeat(0.0, length * 2).ToArray();
                int reward = 0;
                List<int> moves1 = new List<int>();
                List<int> moves2 = new List<int>();
                for (int j = 0; j < 18; j++)
                {
                    var oppstate = state.Skip(length).ToArray();
                    var skipmoves = new HashSet<int>(moves1);
                    // for (int k = 0; k < oppstate.Length; k++)
                    // {
                    //     if (oppstate[k] == 0)
                    //     {
                    //         skipmoves.Add(k);
                    //     }
                    // }
                    // if (skipmoves.Count >= length) {
                    //     skipmoves = moves1.ToHashSet();
                    // }

                    int move1 = random_except_list(length, skipmoves.ToArray());
                    moves1.Add(move1);

                    reward += GetReward(move1, state);

                    int move2 = random_except_list(length, moves2.ToArray());
                    moves2.Add(move2);

                    state[move1] = 1.0;
                    state[move2 + length] = 1.0;
                }
                Rewards.Add(reward);
            }

            logger.LogInformation($"Avg reward: {Rewards.Average()}|{Rewards.Max()}|{Rewards.Min()} {length / 2}");
        }

        public static PaxGame PlayGame(bool genStyle = false)
        {
            
            Player player1 = new Player();
            player1.Team = 1;
            var unit1 = GameUnits.Units.First(f => f.Name == "Marine");
            player1.Units[unit1] = new HashSet<Vector2>() { new Vector2(20, 10) };
            for (int i = 1; i < 20; i++)
            {
                player1.Units[unit1].Add(new Vector2(20, 10 + (i * 20)));
            }

            Player player2 = new Player();
            player2.Team = 2;
            var unit2 = GameUnits.Units.First(f => f.Name == "Zergling");
            player2.Units[unit2] = new HashSet<Vector2>() { new Vector2(960, 15) };
            for (int i = 1; i < 20; i++)
            {
                player2.Units[unit2].Add(new Vector2(960, 200 + (i * 20)));
            }

            PaxGame game = new PaxGame() { GenStyle = genStyle };
            game.Players.Add(player1);
            game.Players.Add(player2);

            // PlayService.Play(game);
            PlayService.PlayAsync(game, logger).GetAwaiter().GetResult();
            return game;
        }

        private static int GetReward(int move, double[] state)
        {
            if (state[move] > 0)
                return 0;
            if (state[move + state.Length / 2] > 0)
                return 2;
            else
                return 1;
        }

        public static int random_except_list(int n, int[] x)
        {
            var range = Enumerable.Range(0, n).Where(i => !x.Contains(i));
            int index = random.Next(0, n - x.Length);
            return range.ElementAt(index);
        }

    }



    public static class ApplicationLogging
    {
        public static ILoggerFactory LogFactory { get; } = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            // Clear Microsoft's default providers (like eventlogs and others)
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            }).SetMinimumLevel(LogLevel.Information);
        });

        public static ILogger<T> CreateLogger<T>() => LogFactory.CreateLogger<T>();
    }
}