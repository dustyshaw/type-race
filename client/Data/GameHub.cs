using client.Data;
using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{

    public async Task StartGame(Player player1, Player player2)
    {
        await Clients.User(player1.Name).SendAsync("GameStarted", player1, player2);
        await Clients.User(player2.Name).SendAsync("GameStarted", player1, player2);
    }

}
