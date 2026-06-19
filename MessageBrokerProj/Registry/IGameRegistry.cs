namespace MessageBrokerProj;

public interface IGameRegistry
{
    bool TryGetGame(string gameId, out GameContext gameContext);
}
