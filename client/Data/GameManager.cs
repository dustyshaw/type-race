using Microsoft.Extensions.FileProviders;

namespace client.Data;

public class GameManager
{
    private Dictionary<string, Game> _games { get; set; } = [];

    public Dictionary<string, Game> GetAllGames() => _games;

    public void CreateGame(string gameId, Player player1, Player player2)
    {
        _games[gameId] = new Game()
        {
            PlayerOne = player1,
            PlayerTwo = player2
        };

        Console.WriteLine("GameManager Created Game with GameId: " + gameId);
    }

    public Game GetGame(string gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }

    public bool UpdateProgress(string gameId, string playerId, int progress)
    {
        _games.TryGetValue(gameId, out var game);

        if (game is null)
        {
            Console.WriteLine("ERROR: Game was not found when attempting to update game state in GameManager.cs");
            return false;
        }

        if (playerId == game.PlayerOne.PlayerId.ToString())
        {
            game.PlayerOneProgress = progress;
        }
        else if (playerId == game.PlayerTwo.PlayerId.ToString())
        {
            game.PlayerTwoProgress = progress;
        }
        else
            return false;

        return true;
    }
}


public class Game
{
    public Player PlayerOne { get; set; }
    public Player PlayerTwo { get; set; }
    public int PlayerOneProgress { get; set; }
    public int PlayerTwoProgress { get; set; }
}
