using client.Data;
using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    public Player PlayerOne { get; set; }
    public Player PlayerTwo { get; set; }

    private int PlayerOneProgress { get; set; }
    private int PlayerTwoProgress { get; set; }

    private readonly GameManager _gameManager;
    public GameHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task StartGameRequest(string gameId, Player player1, Player player2)
    {
        _gameManager.CreateGame(gameId, player1, player2);

        await Clients.All.SendAsync("GameStartedMessage", player2.ConnectionId.ToString());
        await Clients.All.SendAsync("GameStartedMessage", player1.ConnectionId.ToString());
    }

    public async Task RecieveUpdatedGameState(string gameId, GameStateUpdateRequest request)
    {
        Console.WriteLine("I Recieved an updated game state request from playerid: " + request.PlayerId.ToString() + " with a correct count of " + request.Progress.ToString());

        var success = _gameManager.UpdateProgress(gameId, request.PlayerId, request.Progress);

        if (!success)
        {
            Console.WriteLine($"Error: Invalid game or player for gameId {gameId}");
            return;
        }

        var game = _gameManager.GetGame(gameId);
        if (game == null) return;

        var state1 = new GameState
        {
            PlayerId = game.PlayerOne.PlayerId.ToString(),
            Player1Progress = game.PlayerOneProgress,
            Player2Progress = game.PlayerTwoProgress
        };

        var state2 = new GameState
        {
            PlayerId = game.PlayerTwo.PlayerId.ToString(),
            Player1Progress = game.PlayerTwoProgress,
            Player2Progress = game.PlayerOneProgress
        };

        await Clients.All.SendAsync("UpdatedGameState", state1);
        await Clients.All.SendAsync("UpdatedGameState", state2);
    }

    public async Task BroadcastUpdatedState()
    {
        GameState state = new()
        {
            PlayerId = PlayerOne.PlayerId.ToString(),
            Player1Progress = PlayerOneProgress,
            Player2Progress = PlayerTwoProgress
        };

        GameState state2 = new()
        {
            PlayerId = PlayerTwo.PlayerId.ToString(),
            Player1Progress = PlayerTwoProgress,
            Player2Progress = PlayerOneProgress // maybe??

        };

        Console.WriteLine("Broadcasting to all players");
        await Clients.All.SendAsync("UpdatedGameState", state);
        await Clients.All.SendAsync("UpdatedGameState", state2);

    }
}

public record GameState
{
    public string PlayerId { get; set; }
    public int Player1Progress { get; set; }
    public int Player2Progress { get; set; }
}

public record GameStateUpdateRequest()
{
    public int Progress { get; set; }
    public string PlayerId { get; set; }
}