using Microsoft.AspNetCore.SignalR;

namespace client.Data
{
    public class TypeHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
