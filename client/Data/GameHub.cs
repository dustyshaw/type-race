using client.Data;
using Microsoft.AspNetCore.SignalR;

public class GameHub : Hub
{
    public Player PlayerOne { get; set; }
    public Player PlayerTwo { get; set; }

    public async Task StartGameRequest(Player player1, Player player2)
    {
        Console.WriteLine("In the GameHub recieved StartGameRequest!! \nplayer1id: " + player1.ConnectionId + "\nplayer2id: " + player2.ConnectionId);

        PlayerOne = player1;
        PlayerTwo = player2;

        await Clients.All.SendAsync("GameStartedMessage", player2.ConnectionId.ToString());
        await Clients.All.SendAsync("GameStartedMessage", player1.ConnectionId.ToString());
    }

}
