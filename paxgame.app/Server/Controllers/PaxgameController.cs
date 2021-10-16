using Game.Models;
using Game.Services;
using Game.Units;
using Microsoft.AspNetCore.Mvc;
using paxgame.app.Services;
using paxgame.Shared;
using paxgame.Shared.Services;
using System.Diagnostics;
using System.Numerics;

namespace paxgame.app.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaxgameController : ControllerBase
    {
        private readonly ILogger<PaxgameController> logger;

        public PaxgameController(ILogger<PaxgameController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GameResult>> Get()
        {
            // await aiService.TestAi("dqntest_36");


            Player player1 = new Player();
            player1.Team = 1;
            //var unit1 = PaxGameService.GameUnits.Units.First(f => f.Name == "Marine");
            //player1.Units[unit1] = new List<Vector2>() { new Vector2(20, 10) };
            //for (int i = 1; i < 20; i++)
            //{
            //    player1.Units[unit1].Add(new Vector2(20, 10 + (i * 30)));
            //}

            Player player2 = new Player();
            player2.Team = 2;
            //var unit2 = PaxGameService.GameUnits.Units.First(f => f.Name == "Zergling");
            //player2.Units[unit2] = new List<Vector2>() { new Vector2(960, 15) };
            //for (int i = 1; i < 15; i++)
            //{
            //    player2.Units[unit2].Add(new Vector2(940, 10 + (i * 30)));
            //    player2.Units[unit2].Add(new Vector2(920, 10 + (i * 30)));
            //    player2.Units[unit2].Add(new Vector2(900, 10 + (i * 30)));
            //}

            PaxGame game = new PaxGame();
            game.GenStyle = true;
            game.Players.Add(player1);
            game.Players.Add(player2);

            int maxValue = 1000;
            double cost = maxValue;
            while (cost > 0)
            {
                cost -= PaxGameService.PlayRandomMove(game, 0);
            }
            cost = maxValue;
            while (cost > 0)
            {
                cost -= PaxGameService.PlayRandomMove(game, 1);
            }

            player1.AvailableAbilities = player1.Units.Keys.SelectMany(s => s.Abilities).ToHashSet();
            var stim = player1.AvailableAbilities.FirstOrDefault(f => f.AbilityName == AbilityName.Stim);
            var cs = player1.AvailableAbilities.FirstOrDefault(f => f.AbilityName == AbilityName.ConcussiveShells);
            var shield = player1.AvailableAbilities.FirstOrDefault(f => f.AbilityName == AbilityName.CombatShield);
            if (stim != null)
            {
                player1.Abilities.Add(stim);
            }
            if (cs != null)
            {
                player1.Abilities.Add(cs);
            }
            if (shield != null)
            {
                player1.Abilities.Add(shield);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await PlayService.PlayAsync(game, logger);

            sw.Stop();

            logger.LogInformation($"Play round in {sw.ElapsedMilliseconds}ms");

            GameResult result = new GameResult()
            {
                X = game.X,
                Y = game.Y,
                Loop = game.Gameloop,
                Step = game.Step,
                StepTimespan = TimeSpan.FromMilliseconds(8),
                ArmyValueKilledTeam1 = game.ArmyValueKilledTeam1,
                ArmyValueKilledTeam2 = game.ArmyValueKilledTeam2,
                Units = game.BattleUnits.Select(s => s.ViewModel()).Concat(game.DisabledUnits.Select(s => s.ViewModel())).ToList()
            };

            sw.Reset();
            sw.Start();

            var style = StyleService.GenStyle(result, 1000, 600, TimeSpan.FromSeconds(15));
            result.Style = StringCompression.Compress(style);
            sw.Stop();
            logger.LogInformation($"Game Style in {sw.ElapsedMilliseconds}ms ({style.Length})");
            // result.Units.Where(x => x.Path != null).Select(s => s.Path).ToList().ForEach(f => f.Clear());
            // System.IO.File.WriteAllText("/temp/style.txt", style);
            return Ok(result);
        }

        [HttpGet]
        [Route("units/{race}")]
        public async Task<ActionResult<List<UnitResult>>> GetUnits(string race)
        {
            object? eRace;
            if (Enum.TryParse(typeof(Race), race, true, out eRace))
            {
                var units = PaxGameService.GameUnits.Units.Where(x => x.Race == (Race)eRace).ToList();
                if (units.Any())
                {
                    return units.Select(s => new BattleUnit(s, new HashSet<Ability>(), 0, Vector2.Zero, 1, 0).ViewModel()).ToList();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("play")]
        public async Task<ActionResult<GameResult>> Play([FromBody] GameResultRequest request)
        {
            Player player1 = new Player() { Team = 1 };
            Player player2 = new Player() { Team = 2 };

            PaxGame game = new PaxGame();
            game.GenStyle = true;
            game.Players.Add(player1);
            game.Players.Add(player2);

            double minerals = 0;
            foreach (var move in request.moves[0])
            {
                (int unit, myVector2 pos) = AiService.GetUnitPos(move);
                var myUnit = PaxGameService.GameUnits.Units.First(f => f.Race == Race.Terran && f.Id == unit);
                myVector2 boardPos = new myVector2() { Y = pos.X * 20f, X = pos.Y * 20f };
                var boardPosV = GameService.mirrorImage(0, -1, game.Y / 2, boardPos.Pos);
                boardPosV.Y -= 50;
                if (!player1.Units.ContainsKey(myUnit))
                {
                    player1.Units[myUnit] = new List<Vector2>() { boardPosV };
                }
                else
                {
                    player1.Units[myUnit].Add(boardPosV);
                }
                minerals += myUnit.Cost;
            }



            double oppMinerals = 0;
            HashSet<int> oppMoves = new HashSet<int>();

            foreach (var move in request.moves[1])
            {
                (int unit, myVector2 pos) = AiService.GetUnitPos(move);
                var myUnit = PaxGameService.GameUnits.Units.First(f => f.Race == Race.Zerg && f.Id == unit);
                myVector2 boardPos = new myVector2() { Y = pos.X * 20f, X = pos.Y * 20f };
                var boardPosV = GameService.mirrorImage(-1, 0, game.X / 2, boardPos.Pos);
                boardPosV.Y += 50;
                if (!player2.Units.ContainsKey(myUnit))
                {
                    player2.Units[myUnit] = new List<Vector2>() { boardPosV };
                }
                else
                {
                    player2.Units[myUnit].Add(boardPosV);
                }
                oppMoves.Add(move);
                oppMinerals += myUnit.Cost;
            }

            while (oppMinerals < minerals)
            {
                // int move = PaxGameService.random.Next(0, (AiService.buildX - 1) * (AiService.buildY - 1) * AiService.ZergUnits);
                int move = PaxGameService.random_except_list((AiService.buildX - 1) * (AiService.buildY - 1) * AiService.ZergUnits, AiService.GetExceptList(oppMoves, "Zerg"));
                oppMoves.Add(move);
                (int unit, myVector2 pos) = AiService.GetUnitPos(move);
                var myUnit = PaxGameService.GameUnits.Units.First(f => f.Race == Race.Zerg && f.Id == unit);
                myVector2 boardPos = new myVector2() { Y = pos.X * 20f, X = pos.Y * 20f };
                var boardPosV = GameService.mirrorImage(-1, 0, game.X / 2, boardPos.Pos);
                boardPosV.Y += 50;
                if (!player2.Units.ContainsKey(myUnit))
                {
                    player2.Units[myUnit] = new List<Vector2>() { boardPosV };
                }
                else
                {
                    player2.Units[myUnit].Add(boardPosV);
                }
                oppMinerals += myUnit.Cost;
            }

            await PlayService.PlayAsync(game, logger);
            // PlayService.Play(game);

            GameResult result = new GameResult()
            {
                X = game.X,
                Y = game.Y,
                Loop = game.Gameloop,
                Step = game.Step,
                StepTimespan = TimeSpan.FromMilliseconds(8),
                ArmyValueKilledTeam1 = game.ArmyValueKilledTeam1,
                ArmyValueKilledTeam2 = game.ArmyValueKilledTeam2,
                Units = game.BattleUnits.Select(s => s.ViewModel()).Concat(game.DisabledUnits.Select(s => s.ViewModel())).ToList()
            };
            result.PlayerMoves[0] = request.moves[0].ToList();
            result.PlayerMoves[1] = oppMoves.ToList();

            var style = StyleService.GenStyle(result, request.x, request.y, TimeSpan.FromSeconds(request.duration));

            result.Style = StringCompression.Compress(style);
            return Ok(result);
        }
    }
}
