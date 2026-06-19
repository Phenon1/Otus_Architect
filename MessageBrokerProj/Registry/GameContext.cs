using CommandsProj;

namespace MessageBrokerProj;

public sealed class GameContext
{
    public GameContext(string gameId, QueueICommand commandQueue, string iocScopeId)
    {
        GameId = gameId;
        CommandQueue = commandQueue;
        IoCScopeId = iocScopeId;
    }

    public string GameId { get; }
    public QueueICommand CommandQueue { get; }
    public string IoCScopeId { get; }
}
