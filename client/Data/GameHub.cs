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

    public async Task JoinGame(string gameId)
    {
        Console.WriteLine("Joined Gamee");
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
    }

    public async Task StartGameRequest(string gameId, Player player1, Player player2)
    {
        _gameManager.CreateGame(gameId, player1, player2);

        await Clients.All.SendAsync("GameStartedMessage", player2.ConnectionId.ToString());
        await Clients.All.SendAsync("GameStartedMessage", player1.ConnectionId.ToString());
    }

    public async Task RecieveUpdatedGameState(string gameId, GameStateUpdateRequest request)
    {
        Console.WriteLine("I Recieved an updated game state request from playerid: " + request.PlayerId.ToString() + " with a correct count of " + request.Progress.ToString() + " For GameID " + gameId);

        var success = _gameManager.UpdateProgress(gameId, request.PlayerId, request.Progress);

        if (!success)
        {
            Console.WriteLine($"Error: Invalid game or player for gameId {gameId}");
            return;
        }

        var game = _gameManager.GetGame(gameId);
        if (game == null) return;




        var state2 = new GameState
        {
            PlayerId = game.PlayerTwo.PlayerId.ToString(),
            OpponentProgress = game.PlayerTwoProgress,
        };

        var state1 = new GameState
        {
            PlayerId = game.PlayerOne.PlayerId.ToString(),
            OpponentProgress = game.PlayerOneProgress,
        };
        if (request.PlayerId == PlayerOne.PlayerId.ToString()) // if it is player 1 being updated, then we need to send it to player2
        {

            await Clients.All.SendAsync("UpdatedGameState", state2);
        }
        else if (request.PlayerId == PlayerTwo.PlayerId.ToString())
        {

            await Clients.All.SendAsync("UpdatedGameState", state1);
        }
        else
        {
            Console.WriteLine("Warning: You are attempting to broadcast to a player not within this game");
        }


        //await Clients.Group(gameId).Client(game.PlayerOne.ConnectionId)
        //    .SendAsync("UpdatedGameState", state1);

        //await Clients.Group(gameId).Client(game.PlayerTwo.ConnectionId)
        //    .SendAsync("UpdatedGameState", state2);


        await Clients.All.SendAsync("UpdatedGameState", state1);
        await Clients.All.SendAsync("UpdatedGameState", state2);
    }
}

public record GameState
{
    public string PlayerId { get; set; }
    public int OpponentProgress { get; set; }
    //public int Player2Progress { get; set; }
}

public record GameStateUpdateRequest()
{
    public int Progress { get; set; }
    public string PlayerId { get; set; }
}