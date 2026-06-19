namespace MessageBrokerProj;

public class GameCommandEnqueueException : InvalidOperationException
{
    public GameCommandEnqueueException(string gameId)
        : base($"Команда для игры '{gameId}' не была поставлена в очередь.")
    {
        GameId = gameId;
    }

    public string GameId { get; }
}
