using CommandsProj;
using IoCProj;
using ModelsProj;

namespace MessageBrokerProj;

public sealed class InterpretOrderCommand : ICommand
{
    private readonly IUObject _order;

    public InterpretOrderCommand(IUObject order)
    {
        _order = order;
    }

    public void Execute()
    {
        ExecuteOptionalAuthorization();

        string action = OrderPropertyReader.GetRequired<string>(_order, "action");

        Func<IUObject, ICommand> commandFactory;
        try
        {
            commandFactory = IoC.Resolve<Func<IUObject, ICommand>>(OrderIocKeys.AllowedAction, action);
        }
        catch (KeyNotFoundException ex)
        {
            throw new GameMessageSecurityException($"Действие приказа '{action}' запрещено.", ex);
        }

        commandFactory(_order).Execute();
    }

    private void ExecuteOptionalAuthorization()
    {
        ICommand authorization;
        try
        {
            authorization = IoC.Resolve<ICommand>(OrderIocKeys.Authorize, _order);
        }
        catch (KeyNotFoundException)
        {
            return;
        }

        authorization.Execute();
    }
}
