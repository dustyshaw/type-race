using client.Data;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;


public class PlayerHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, Player> AllPlayers = new();
    private static Dictionary<Guid, string> playerConnections = new();
    private string groupName = "PlayerGroup";

    public async Task ConnectPlayer(string playername)
    {
        var player = new Player(playername, Context.ConnectionId);

        AllPlayers.TryAdd(player.PlayerId, player);
        playerConnections[player.PlayerId] = player.ConnectionId;

        Console.WriteLine($"Player {player.Name} connected with ConnectionId: {player.ConnectionId}");
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

    public async Task RecieveDualRequest(Guid fromId, Guid toId)
    {
        if (!AllPlayers.ContainsKey(fromId) || !AllPlayers.ContainsKey(toId))
        {
            await Clients.Caller.SendAsync("NoPlayerFound");
            return;
        }

        if (!AllPlayers.TryGetValue(toId, out Player? toPlayer))
        {
            await Clients.Caller.SendAsync("NoOpponentFound");
            return;
        }

        // connection id and player name are correct.
        Console.WriteLine($"Opponent found: {toPlayer.Name} with ConnectionId {toPlayer.ConnectionId}");

        if (toPlayer is not null)
        {
            await Clients.Caller.SendAsync("WaitingForOpponentConfirmation", toPlayer.PlayerId);

            // These logs look correct. I am sending it to the correct person
            Console.WriteLine("Sending a message to connectionId " + toPlayer.ConnectionId);
            await Clients.User(AllPlayers[fromId].ConnectionId).SendAsync("SendRequestToOpponent", "yay");
            await Clients.Client(AllPlayers[fromId].ConnectionId).SendAsync("SendRequestToOpponent", "yay");
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
