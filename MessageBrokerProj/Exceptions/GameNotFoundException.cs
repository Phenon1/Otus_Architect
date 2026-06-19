namespace MessageBrokerProj;

public class GameNotFoundException : KeyNotFoundException
{
    public GameNotFoundException(string gameId)
        : base($"Игра с идентификатором '{gameId}' не найдена.")
    {
        GameId = gameId;
    }

    public string GameId { get; }
}
