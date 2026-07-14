using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using CommandsProj;
using CommandsProj.Commands;
using IoCProj;
using MessageBrokerProj;
using ModelsProj;
using ModelsProj.TypesObject;

namespace Tests;

public class OrderInterpreterTest
{
    [Test]
    public void InterpretOrderExecutesStartStopAndShootActionsRegisteredInIoc()
    {
        CreateScope("order-actions");
        List<string> executed = [];
        Dictionary<string, Func<IUObject, ICommand>> actions = new()
        {
            ["StartMove"] = order => new ActionCommand(() =>
                executed.Add($"start:{order.GetProperty<int>("initialVelocity")}")),
            ["StopMove"] = _ => new ActionCommand(() => executed.Add("stop")),
            ["Shoot"] = order => new ActionCommand(() =>
                executed.Add($"shoot:{order.GetProperty<string>("weaponType")}"))
        };

        Register(OrderIocKeys.AllowedAction, args => actions[(string)args[0]]);

        new InterpretOrderCommand(Order(("id", "ship-1"), ("action", "StartMove"), ("initialVelocity", 2))).Execute();
        new InterpretOrderCommand(Order(("id", "ship-1"), ("action", "StopMove"))).Execute();
        new InterpretOrderCommand(Order(("id", "ship-1"), ("action", "Shoot"), ("weaponType", "laser"))).Execute();

        Assert.That(executed, Is.EqualTo(new[] { "start:2", "stop", "shoot:laser" }));
    }

    [Test]
    public void InterpretOrderExecutesNonGameOrdersWithoutCodeChanges()
    {
        CreateScope("system-order");
        bool saved = false;
        Register(OrderIocKeys.AllowedAction, args =>
        {
            Assert.That(args[0], Is.EqualTo("SaveGame"));
            return (Func<IUObject, ICommand>)(order => new ActionCommand(() =>
            {
                Assert.That(order.GetProperty<string>("saveName"), Is.EqualTo("slot-1"));
                saved = true;
            }));
        });

        new InterpretOrderCommand(Order(("action", "SaveGame"), ("saveName", "slot-1"))).Execute();

        Assert.That(saved, Is.True);
    }

    [Test]
    public void PlayerScopedOrderAuthorizationAllowsOnlyOwnedObjects()
    {
        string gameScope = CreateScope("order-game");
        string player1Scope = CreatePlayerScope("order-player-1", "ship-1", new SpaceShip());
        string player2Scope = CreatePlayerScope("order-player-2", "ship-2", new SpaceShip());
        UseScope(gameScope);

        Dictionary<string, string> playerScopes = new()
        {
            ["player-1"] = player1Scope,
            ["player-2"] = player2Scope
        };
        bool executed = false;

        Register(OrderIocKeys.PlayerScope, args => playerScopes[(string)args[0]]);
        Register(OrderIocKeys.Authorize, args => new PlayerScopedOrderAuthorizationCommand((IUObject)args[0]));
        Register(OrderIocKeys.AllowedAction, _ =>
            (Func<IUObject, ICommand>)(_ => new ActionCommand(() => executed = true)));

        new InterpretOrderCommand(Order(
            ("playerId", "player-1"),
            ("id", "ship-1"),
            ("action", "StartMove"))).Execute();

        Assert.That(executed, Is.True);

        Assert.Throws<GameMessageSecurityException>(() => new InterpretOrderCommand(Order(
            ("playerId", "player-2"),
            ("id", "ship-1"),
            ("action", "StartMove"))).Execute());
    }

    [Test]
    public void InterpretCommandUsesPlayerScopesToProtectGameMessages()
    {
        string gameScope = CreateScope("message-game");
        string player1Scope = CreatePlayerScope("message-player-1", "ship-1", new SpaceShip());
        string player2Scope = CreatePlayerScope("message-player-2", "ship-2", new SpaceShip());
        UseScope(gameScope);

        QueueICommand queue = new();
        EmptyCommand targetCommand = new();
        Dictionary<string, string> playerScopes = new()
        {
            ["player-1"] = player1Scope,
            ["player-2"] = player2Scope
        };

        Register(GameMessageIocKeys.Validate, _ => GameMessageValidationResult.Valid());
        Register(GameMessageIocKeys.PlayerScope, args => playerScopes[(string)args[0]]);
        Register(GameMessageIocKeys.Authorize, args =>
            new GameMessagePlayerScopedAuthorizationCommand((GameMessage)args[0]));
        Register(GameMessageIocKeys.AllowedOperation, _ =>
            (Func<GameMessage, ICommand>)(_ => targetCommand));
        Register(GameMessageIocKeys.Enqueue, args =>
        {
            queue.Enqueue((ICommand)args[1]);
            return true;
        });

        GameContext context = new("game-1", queue, gameScope);
        new InterpretCommand(Message("player-1", "ship-1"), context).Execute();

        Assert.That(queue.TryDequeue(out ICommand? command), Is.True);
        Assert.That(command, Is.SameAs(targetCommand));
        Assert.Throws<GameMessageSecurityException>(() =>
            new InterpretCommand(Message("player-2", "ship-1"), context).Execute());
        Assert.That(queue.IsEmpty, Is.True);
    }

    private static string CreateScope(string prefix)
    {
        string scopeId = $"{prefix}{Guid.NewGuid():N}";
        UseScope("root");
        IoC.Resolve<ICommand>("Scopes.New", scopeId).Execute();
        return scopeId;
    }

    private static string CreatePlayerScope(string prefix, string objectId, IUObject obj)
    {
        string scopeId = CreateScope(prefix);
        Register(OrderIocKeys.Objects, args =>
            (string)args[0] == objectId
                ? obj
                : throw new KeyNotFoundException($"Объект '{args[0]}' не найден."));
        return scopeId;
    }

    private static void UseScope(string scopeId) =>
        IoC.Resolve<ICommand>("Scopes.Current", scopeId).Execute();

    private static void Register(string key, IoC.DependencyStrategy strategy) =>
        IoC.Resolve<ICommand>("IoC.Register", key, strategy).Execute();

    private static TestOrder Order(params (string Key, object Value)[] properties)
    {
        TestOrder order = new();
        foreach ((string key, object value) in properties)
        {
            order.SetProperty(key, value);
        }

        return order;
    }

    private static GameMessage Message(string playerId, string objectId)
    {
        return new GameMessage
        {
            GameId = "game-1",
            Token = Token(playerId),
            ObjectId = objectId,
            OperationId = "StartMove",
            Args = JsonDocument.Parse("""{"initialVelocity":2}""").RootElement.Clone()
        };
    }

    private static string Token(string playerId)
    {
        JwtSecurityToken token = new(
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, playerId)
            ]);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class TestOrder : Uobject
    {
    }

    private sealed class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public void Execute() => _action();
    }
}
