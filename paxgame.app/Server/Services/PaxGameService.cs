
using Game.Models;
using Game.Services;
using Game.Units;
using System.Collections.Concurrent;
using System.Numerics;

namespace paxgame.app.Services;
public static class PaxGameService
{
    public static GameUnits GameUnits = new GameUnits();

    public static ConcurrentDictionary<int, PaxGame> Games = new ConcurrentDictionary<int, PaxGame>();
    public static Random random = new Random();

    public static void CreateGame(int gameId)
    {
        Player player1 = new Player();
        player1.Team = 1;

        Player player2 = new Player();
        player2.Team = 2;

        PaxGame game = new PaxGame();
        game.X = 500;
        game.Y = 300;
        game.Players.Add(player1);
        game.Players.Add(player2);

        Games.TryAdd(gameId, game);
    }

    public static void PlayMove(int gameId, int playerId, int move)
    {
        var game = Games[gameId];
        var player = game.Players[playerId];

        if (player.Moves.Contains(move))
        {
            return;
        }

        var unit1 = PaxGameService.GameUnits.Units.First(f => f.Name == "Marine");
        Vector2 buildPos = GetPos(move, game.BX, game.BY);
        Vector2 boardPos = new Vector2(x: buildPos.X, y: (game.Y - game.BY) / 2 + buildPos.Y);
        if (player.Team == 2)
        {
            boardPos = GameService.mirrorImage(-1, 0, game.X / 2, boardPos);
        }
        if (!player.Units.ContainsKey(unit1))
        {
            player.Units[unit1] = new HashSet<Vector2>() { boardPos };
        }
        else
        {
            player.Units[unit1].Add(boardPos);
        }
        player.Moves.Add(move);
    }

    public static void PlayRandomMove(int gameId, int playerId)
    {
        var game = Games[gameId];
        PlayRandomMove(game, playerId);
    }

    public static double PlayRandomMove(PaxGame game, int playerId)
    {
        var player = game.Players[playerId];

        Unit unit1;
        if (player.Team == 1)
        {
            int unitid = random.Next(1, 4);
            unit1 = GameUnits.Units.First(f => f.Race == Race.Terran && f.Id == unitid);
        } else
        {
            int unitid = random.Next(1, 5);
            unit1 = GameUnits.Units.First(f => f.Race == Race.Zerg && f.Id == unitid);
        }

        //var unit1 = player.Team == 1 ?
        //    PaxGameService.GameUnits.Units.First(f => f.Name == "Marine")
            
        //    : PaxGameService.GameUnits.Units.First(f => f.Name == "Zergling");

        int move = random_except_list(game.BX * game.BY - 1, player.Moves);

        Vector2 buildPos = GetPos(move, game.BX, game.BY);
        Vector2 boardPos = new Vector2(x: buildPos.X * 20, y: (game.Y - game.BY) / 2 + buildPos.Y * 20);
        if (player.Team == 2)
        {
            boardPos = GameService.mirrorImage(-1, 0, game.X / 2, boardPos);
        }
        if (!player.Units.ContainsKey(unit1))
        {
            player.Units[unit1] = new HashSet<Vector2>() { boardPos };
        }
        else
        {
            player.Units[unit1].Add(boardPos);
        }
        player.Moves.Add(move);
        return unit1.Cost;
    }

    private static (int, Vector2) GetUnitAndPos(int move, int x, int y)
    {
        int pos, unit = Math.DivRem(move, x * y, out pos);
        return (unit, GetPos(pos, x, y));
    }

    private static Vector2 GetPos(int pos, int x, int y)
    {
        int uy, ux = Math.DivRem(pos, y, out uy);
        return new Vector2(ux, uy);
    }

    public static int random_except_list(int n, HashSet<int> x)
    {
        var range = Enumerable.Range(0, n).Where(i => !x.Contains(i));
        int index = random.Next(0, n - x.Count);
        return range.ElementAt(index);
    }
}
