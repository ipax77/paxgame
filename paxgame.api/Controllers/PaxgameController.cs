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
    public async Task<ActionResult<AiResponse>> GenerateResult([FromBody] AiRequest request)
    {
        List<int> State = new List<int>();
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

        await PlayService.PlayAsync(game, _logger);

        var p1 = Math.Round(game.ArmyValueKilledTeam1 * 100.0 / player2.ArmyValue, 2);
        var p2 = Math.Round(game.ArmyValueKilledTeam2 * 100.0 / player1.ArmyValue, 2);

        int reward = Convert.ToInt32(p1 - p2);

        _logger.LogInformation($"result: {p1}|{p2} => {reward}");

        return new AiResponse(player2.Moves.Except(request.moves[1]).ToArray(), reward);
    }
}
