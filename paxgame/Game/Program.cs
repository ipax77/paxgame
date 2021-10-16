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

            logger.LogInformation("Hello World");
            GameUnits gameUnits = new GameUnits();
            
            List<double> playTimes = new List<double>();
            // Console.ReadLine();
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < 10; i++)
            {
                sw.Start();
                PlayGame(true);
                sw.Stop();
                playTimes.Add(sw.ElapsedMilliseconds);
                sw.Reset();
            }
            logger.LogInformation($"Playtimes: {playTimes.Average()}ms ({playTimes.Min()}|{playTimes.Max()})");

            Console.ReadLine();
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
            }).SetMinimumLevel(LogLevel.Error);
        });

        public static ILogger<T> CreateLogger<T>() => LogFactory.CreateLogger<T>();
    }
}