using client.Data;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;


public class PlayerHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, Player> AllPlayers = new();
    private static Dictionary<Guid, string> playerConnections = new();

    public async Task ConnectPlayer(ConnectToAllPlayersRequest request)
    {
        var playerName = request.PlayerName;
        var connectionId = request.ConnectionId;
        var player = new Player(playerName, connectionId);

        AllPlayers.TryAdd(player.PlayerId, player);
        playerConnections[player.PlayerId] = connectionId;

        //Console.WriteLine($"Player {playerName} connected with ConnectionId: {connectionId}");
        await BroadcastPlayers();
    }

    public async Task BroadcastPlayers()
    {
        var players = AllPlayers.Values.ToList();
        await Clients.All.SendAsync("BroadcastPlayers", new UpdatedPlayersListMessage(players));
    }

    public async Task GetPlayers()
    {
        var players = AllPlayers.Values.ToList();
        await Clients.Caller.SendAsync("UpdatedPlayerList", new UpdatedPlayersListMessage(players));
    }

    public async Task RecieveDualRequest(Guid fromId, Guid toId) // toid and from id are the same ???
    {
        Console.WriteLine("\nRecieveDualRequest in PlayerHub");
        if (!AllPlayers.ContainsKey(fromId) || !AllPlayers.ContainsKey(toId))
        {
            await Clients.Caller.SendAsync("NoPlayerFound");
            return;
        }

        if (!AllPlayers.TryGetValue(fromId, out Player? fromPlayer))
        {
            await Clients.Caller.SendAsync("NoOpponentFound");
            return;
        }

        if (!AllPlayers.TryGetValue(toId, out Player? toPlayer))
        {
            await Clients.Caller.SendAsync("NoOpponentFound");
            return;
        }

        Console.WriteLine($"Opponent found: {toPlayer.Name} with ConnectionId {toPlayer.ConnectionId}");

        if (toPlayer is not null)
        {
            await Clients.Caller.SendAsync("WaitingForOpponentConfirmation", fromId);

            //Console.WriteLine("Sending a message to connectionId " + toPlayer.ConnectionId);
            Console.WriteLine("toPlayer: " + toPlayer.ConnectionId.ToString());
            Console.WriteLine("fromPlayer: " + fromPlayer.ConnectionId.ToString());
            Console.WriteLine();

            await Clients.All.SendAsync("SendRequestToOpponent", 
                new MyIdAndOpponentIdMessage() {
                    MyConnectionId= toPlayer.ConnectionId, 
                    OpponentPlayerId=fromPlayer.PlayerId
                }
                );
        }
    }


}

public class UpdatedPlayersListMessage
{
    public List<Player> UpdatedPlayers { get; set; }

    public UpdatedPlayersListMessage(List<Player> updatedPlayers)
    {
        UpdatedPlayers = updatedPlayers;
    }
}

public record MyIdAndOpponentIdMessage
{
    public string MyConnectionId { get; set; }
    public Guid OpponentPlayerId { get; set; }
}
