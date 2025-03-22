using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace client.Data;

public class PlayerHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, Player> AllPlayers = new();

    public async Task ConnectPlayer(string playername)
    {
        var player = new Player(playername);
        AllPlayers.TryAdd(player.PlayerId, player);
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
    public Guid PlayerId { get; set; } = Guid.NewGuid();
    public string Name { get; set; }

    public Player(string name)
    {
        Name = name;
    }
}
