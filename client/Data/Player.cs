namespace client.Data;

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
