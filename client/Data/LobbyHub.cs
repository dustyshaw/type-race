using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;

namespace client.Data;

public class LobbyHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, Lobby> ActiveLobbies = new();

    public async Task AskForLobbies()
    {
        var lobbies = ActiveLobbies.Values.Select(l => new UpdatedLobbiesMessage(l.LobbyId, l.Name, l.ConnectedPlayers)).ToList();
        await Clients.Caller.SendAsync("ReceiveUpdatedLobbies", lobbies);
    }

    public async Task CreateLobby(CreateLobbyRequest request)
    {
        ActiveLobbies.TryAdd(request.LobbyId, new Lobby(request.LobbyId, request.LobbyName));
        await BroadcastLobbies();
    }

    public async Task JoinLobby(JoinLobbyRequest request)
    {
        //if (ActiveLobbies.TryGetValue(request.LobbyId, out var requestedLobby))
        //{
        //    var player = new Player(request.PlayerId, "Player " + request.PlayerId);
        //    requestedLobby.AddPlayer(player);

        //    await Clients.Caller.SendAsync("SuccessfullyJoinedLobby", requestedLobby.LobbyId, requestedLobby.Name);

        //    await BroadcastLobbies();
        //}
        //else
        //{
        //    await Clients.Caller.SendAsync("LobbyNotFound", request.LobbyId);
        //}
    }
    private async Task BroadcastLobbies()
    {
        var lobbies = ActiveLobbies.Values.Select(l => new UpdatedLobbiesMessage(l.LobbyId, l.Name, l.ConnectedPlayers)).ToList();
        await Clients.All.SendAsync("ReceiveUpdatedLobbies", lobbies);
    }
}


// messages
public class CreateLobbyRequest
{
    public Guid LobbyId { get; set; }
    public string LobbyName { get; set; }
}

public class JoinLobbyRequest
{
    public Guid LobbyId { get; set; }   
    public Guid PlayerId { get; set; }
}

public class UpdatedLobbiesMessage
{
    public Guid LobbyId { get; set; }
    public string LobbyName { get; set; }
    public List<Player> Players { get; set; }

    public UpdatedLobbiesMessage(Guid lobbyId, string lobbyName, List<Player> players)
    {
        LobbyId = lobbyId;
        LobbyName = lobbyName;
        Players = players;
    }
}

public class Lobby
{
    public Guid LobbyId { get; set; }
    public string Name { get; set; }
    public List<Player> ConnectedPlayers { get; set; }

    public Lobby(Guid NewLobbyId, string NewLobbyName)
    {
        this.LobbyId = NewLobbyId;
        this.Name = NewLobbyName;
        ConnectedPlayers = new List<Player>();
    }

    public void AddPlayer(Player player)
    {
        ConnectedPlayers.Add(player);
    }
}

//public class Player
//{
//    public Guid PlayerId { get; set; }
//    public string Name { get; set; }

//    public Player(string playerName)
//    {
//        this.PlayerId = Guid.NewGuid();
//        this.Name = playerName;
//    }
//}
