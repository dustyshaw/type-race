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
        if(!AllPlayers.ContainsKey(playerId) || !AllPlayers.ContainsKey(opponentId))
        {
            await Clients.Caller.SendAsync("NoPlayerFound");
        }

        if (!AllPlayers.TryGetValue(opponentId, out Player? opponent))
        {
            await Clients.Caller.SendAsync("NoOpponentFound");
        }

        if (opponent is not null)
        {
            await Clients.Client(opponent.ConnectionId).SendAsync("ReceiveDualRequest", playerId);
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
