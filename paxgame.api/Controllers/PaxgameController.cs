using Game.Models;
using Game.Services;
using Microsoft.AspNetCore.Mvc;
using paxgame.Shared;

namespace paxgame.api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaxgameController : ControllerBase
{
    private readonly ILogger<PaxgameController> _logger;

    public PaxgameController(ILogger<PaxgameController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        

        return Ok("Hello World");
    }

    [HttpPost]
    public async Task<ActionResult<double>> GenerateResult([FromBody] AiRequest request)
    {
        Player player1 = new Player() { Team = 1, Race = Race.Terran };
        Player player2 = new Player() { Team = 2, Race = Race.Zerg };

        PaxGame game = new PaxGame();
        game.GenStyle = false;
        game.Players.Add(player1);
        game.Players.Add(player2);

        player1.AddMoves(request.moves[0]);
        player2.AddMoves(request.moves[1]);

        //while (player2.Minerals < player1.Minerals)
        //{
        //    player2.AddRandomMove();
        //}
        //if (player2.Minerals > player1.Minerals)
        //{
        //    player2.UndoLastMove();
        //}

        await PlayService.PlayAsync(game, _logger);

        var p1 = player2.ArmyValue == 0 ? 0 : game.ArmyValueKilledTeam1 / player2.ArmyValue;
        // var p2 = player1.ArmyValue == 0 ? 0 : Math.Round(game.ArmyValueKilledTeam2 * 100.0 / player1.ArmyValue, 2);

        //int reward = Convert.ToInt32(p1 - p2);
        double reward = 1.0;
        if (game.ArmyValueKilledTeam1 > game.ArmyValueKilledTeam2)
        {
            reward =  p1 > 0.65 ? 3.0 : 2.0;
        }
        return reward;
    }

    [HttpPost]
    [Route("moves")]
    public ActionResult<int[]> GetDotnetMoves([FromBody] AiRequest request)
    {
        Player player1 = new Player() { Team = 1, Race = Race.Terran };
        Player player2 = new Player() { Team = 2, Race = Race.Zerg };

        PaxGame game = new PaxGame();
        game.GenStyle = false;
        game.Players.Add(player1);
        game.Players.Add(player2);

        player1.AddMoves(request.moves[0]);
        player2.AddMoves(request.moves[1]);

        while (player2.Minerals < player1.Minerals)
        {
            player2.AddRandomMove();
        }
        if (player2.Minerals > player1.Minerals)
        {
            player2.UndoLastMove();
        }
        return player2.Moves.Except(request.moves[1]).ToArray();
    }
}
