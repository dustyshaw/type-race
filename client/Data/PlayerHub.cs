using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace client.Data;

public class PlayerHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, Player> AllPlayers = new();
    private static Dictionary<Guid, string> playerConnections = new();

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

    public async Task RecieveDualRequest(Guid playerId, Guid opponentId)
    {
        if (!AllPlayers.ContainsKey(playerId) || !AllPlayers.ContainsKey(opponentId))
        {
            await Clients.Caller.SendAsync("NoPlayerFound");
            return;
        }

        if (!AllPlayers.TryGetValue(opponentId, out Player? opponent))
        {
            await Clients.Caller.SendAsync("NoOpponentFound");
            return;
        }

        // Log to verify the opponent's ConnectionId
        Console.WriteLine($"Opponent found: {opponent.Name} with ConnectionId {opponent.ConnectionId}");

        if (opponent is not null)
        {
            // Send the message to the correct opponent's client using their ConnectionId

            await Clients.All.SendAsync("SendRequestToOpponent", opponent.PlayerId);
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

public class Player
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; }
    public PlayerStateEnum status { get; set; }
    public string ConnectionId { get; set; }

    public Player(string name, string connectionId)
    {
        Name = name;
        status = PlayerStateEnum.WaitingToJoin;
        PlayerId = Guid.NewGuid();
        ConnectionId = connectionId;
    }
}
