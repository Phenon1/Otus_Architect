using CommandsProj;
using ModelsProj;

namespace MessageBrokerProj;

public sealed class PlayerScopedOrderAuthorizationCommand : ICommand
{
    private readonly IUObject _order;

    public PlayerScopedOrderAuthorizationCommand(IUObject order)
    {
        _order = order;
    }

    public void Execute()
    {
        string playerId = OrderPropertyReader.GetRequired<string>(_order, "playerId");
        string objectId = OrderPropertyReader.GetRequiredAny<string>(_order, "id", "ID");

        PlayerScopedObjectOwnership.EnsureOwned(
            playerId,
            objectId,
            OrderIocKeys.PlayerScope,
            OrderIocKeys.Objects);
    }
}
