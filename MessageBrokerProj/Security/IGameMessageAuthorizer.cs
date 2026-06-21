namespace MessageBrokerProj;

public interface IGameMessageAuthorizer
{
    GameMessageValidationResult Authorize(GameMessage message);
}
