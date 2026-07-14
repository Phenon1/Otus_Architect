namespace MessageBrokerProj;

public static class GameMessageIocKeys
{
    public const string Validate = "game.message.validate";
    public const string Authorize = "game.message.authorize";
    public const string AllowedOperation = "game.operations.allowed";
    public const string Enqueue = "game.commands.enqueue";
    public const string PlayerScope = "game.players.scope";
}
