using System.Collections.Concurrent;

namespace MessageBrokerProj;

public sealed class InMemoryGameRegistry : IGameRegistry
{
    private readonly ConcurrentDictionary<string, GameContext> _games = new();

    public bool TryRegister(GameContext gameContext)
    {
        return _games.TryAdd(gameContext.GameId, gameContext);
    }

    public bool TryRemove(string gameId)
    {
        return _games.TryRemove(gameId, out _);
    }

    public bool TryGetGame(string gameId, out GameContext gameContext)
    {
        return _games.TryGetValue(gameId, out gameContext!);
    }
}
