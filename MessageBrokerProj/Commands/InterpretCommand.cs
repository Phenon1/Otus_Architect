using CommandsProj;
using IoCProj;

namespace MessageBrokerProj;

public sealed class InterpretCommand : ICommand
{
    private readonly GameMessage _message;
    private readonly GameContext _gameContext;

    public InterpretCommand(GameMessage message, GameContext gameContext)
    {
        _message = message;
        _gameContext = gameContext;
    }

    public void Execute()
    {
        IoC.Resolve<ICommand>("Scopes.Current", _gameContext.IoCScopeId).Execute();

        GameMessageValidationResult validationResult =
            IoC.Resolve<GameMessageValidationResult>(GameMessageIocKeys.Validate, _message);

        if (!validationResult.IsValid)
        {
            throw new GameMessageSecurityException(validationResult.Error ?? "Сообщение не прошло проверку безопасности.");
        }

        Func<GameMessage, ICommand> commandFactory;
        try
        {
            commandFactory =
                IoC.Resolve<Func<GameMessage, ICommand>>(GameMessageIocKeys.AllowedOperation, _message.OperationId);
        }
        catch (KeyNotFoundException ex)
        {
            throw new GameMessageSecurityException($"Операция '{_message.OperationId}' запрещена.", ex);
        }

        ICommand targetCommand = commandFactory(_message);
        bool success = IoC.Resolve<bool>(GameMessageIocKeys.Enqueue, _message.GameId, targetCommand);

        if (!success)
        {
            throw new GameCommandEnqueueException(_message.GameId);
        }
    }
}
