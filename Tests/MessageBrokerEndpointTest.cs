using CommandsProj;
using CommandsProj.Commands;
using IoCProj;
using MessageBrokerProj;
using ModelsProj;
using ModelsProj.TypesObject;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Tests;

public class MessageBrokerEndpointTest
{
    private const string SigningKey = "message-broker-test-signing-key-32-bytes-minimum";
    private const string WrongSigningKey = "wrong-message-broker-signing-key-32-bytes";
    private const string Issuer = "test-authorization-service";
    private const string Audience = "test-game-server";

    [Test]
    public void EndpointRoutesMessage()
    {
        QueueICommand queue = new QueueICommand();
        InMemoryGameRegistry registry = new InMemoryGameRegistry();
        registry.TryRegister(new GameContext("game-1", queue, "scope-game-1"));

        Delivery delivery = new Delivery(ValidJson());
        Endpoint(registry).HandleDelivery(delivery);

        Assert.That(delivery.Acked, Is.True);
        Assert.That(delivery.Nacked, Is.False);
        Assert.That(queue.TryDequeue(out ICommand? command), Is.True);
        Assert.That(command, Is.InstanceOf<InterpretCommand>());
        Assert.That(Response(delivery).Success, Is.True);
    }

    [Test]
    public void EndpointRejectsBadMessages()
    {
        Delivery badJson = new Delivery("{ not json");
        Endpoint(new InMemoryGameRegistry()).HandleDelivery(badJson);

        Delivery unknownGame = new Delivery(ValidJson());
        Endpoint(new InMemoryGameRegistry()).HandleDelivery(unknownGame);

        AssertRejected(badJson);
        AssertRejected(unknownGame);
    }

    [Test]
    public void EndpointRejectsUnauthorizedMessagesBeforeEnqueue()
    {
        QueueICommand queue = new QueueICommand();
        InMemoryGameRegistry registry = new InMemoryGameRegistry();
        registry.TryRegister(new GameContext("game-1", queue, "scope-game-1"));
        RabbitMqGameMessageEndpoint endpoint = Endpoint(registry);

        Delivery[] deliveries =
        [
            new(ValidJson(token: string.Empty)),
            new(ValidJson(token: CreateToken("user-1", "game-1", signingKey: WrongSigningKey))),
            new(ValidJson(token: CreateToken(
                "user-1",
                "game-1",
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1)))),
            new(ValidJson(token: CreateToken("user-1", "another-game")))
        ];

        foreach (Delivery delivery in deliveries)
        {
            endpoint.HandleDelivery(delivery);
            AssertRejected(delivery);
        }

        Assert.That(queue.IsEmpty, Is.True);
    }

    [Test]
    public void InterpretCommandUsesWhitelist()
    {
        GameContext context = Context(out QueueICommand queue);
        SpaceShip ship = new SpaceShip();
        EmptyCommand targetCommand = new EmptyCommand();

        RegisterValidation(GameMessageValidationResult.Valid());
        RegisterObject("ship-1", ship);
        RegisterOperation("startMove", message =>
        {
            Assert.That(IoC.Resolve<IUObject>("Game.Objects", message.ObjectId), Is.SameAs(ship));
            Assert.That(message.Args.GetProperty("velocity").GetInt32(), Is.EqualTo(2));
            return targetCommand;
        });
        RegisterEnqueue(queue);

        new InterpretCommand(Message(ValidJson()), context).Execute();

        Assert.That(queue.TryDequeue(out ICommand? command), Is.True);
        Assert.That(command, Is.SameAs(targetCommand));
    }

    [Test]
    public void InterpretCommandRejectsUnsafeMessages()
    {
        GameContext context = Context(out QueueICommand queue);
        RegisterValidation(GameMessageValidationResult.Valid());
        RegisterObject("ship-1", new SpaceShip());
        Assert.Throws<GameMessageSecurityException>(() => new InterpretCommand(Message(ValidJson()), context).Execute());
        Assert.That(queue.IsEmpty, Is.True);

        context = Context(out queue);
        RegisterValidation(GameMessageValidationResult.Valid());
        RegisterOperation("startMove", _ => throw new GameMessageSecurityException("Аргумент 'velocity' обязателен."));
        RegisterEnqueue(queue);

        Assert.Throws<GameMessageSecurityException>(() => new InterpretCommand(Message(ValidJson()), context).Execute());
        Assert.That(queue.IsEmpty, Is.True);
    }

    private sealed class Delivery : IBrokerDelivery
    {
        public Delivery(string body) => Body = Encoding.UTF8.GetBytes(body);

        public ReadOnlyMemory<byte> Body { get; }
        public ulong DeliveryTag => 1;
        public string ReplyTo => "agent.reply";
        public string CorrelationId => "corr-1";
        public bool Acked { get; private set; }
        public bool Nacked { get; private set; }
        public byte[]? PublishedResponse { get; private set; }

        public void Ack() => Acked = true;
        public void Nack(bool requeue) => Nacked = !requeue;
        public void PublishResponse(ReadOnlyMemory<byte> body) => PublishedResponse = body.ToArray();
    }

    private static RabbitMqGameMessageEndpoint Endpoint(IGameRegistry registry) =>
        new RabbitMqGameMessageEndpoint(
            registry,
            new JwtGameMessageAuthorizer(new JwtGameMessageAuthorizerOptions
            {
                SigningKey = SigningKey,
                Issuer = Issuer,
                Audience = Audience
            }),
            new SystemTextJsonIncomingMessageSerializer());

    private static GameContext Context(out QueueICommand queue)
    {
        string scopeId = "game-scope-" + Guid.NewGuid().ToString("N");
        queue = new QueueICommand();
        IoC.Resolve<ICommand>("Scopes.Current", "root").Execute();
        IoC.Resolve<ICommand>("Scopes.New", scopeId).Execute();
        return new GameContext("game-1", queue, scopeId);
    }

    private static void RegisterValidation(GameMessageValidationResult result) =>
        IoC.Resolve<ICommand>("IoC.Register", GameMessageIocKeys.Validate, (IoC.DependencyStrategy)(_ => result)).Execute();

    private static void RegisterObject(string objectId, IUObject obj) =>
        IoC.Resolve<ICommand>("IoC.Register", "Game.Objects", (IoC.DependencyStrategy)(args =>
            (string)args[0] == objectId ? obj : throw new KeyNotFoundException($"Объект '{args[0]}' не найден."))).Execute();

    private static void RegisterOperation(string operationId, Func<GameMessage, ICommand> factory) =>
        IoC.Resolve<ICommand>("IoC.Register", GameMessageIocKeys.AllowedOperation, (IoC.DependencyStrategy)(args =>
            (string)args[0] == operationId ? factory : throw new KeyNotFoundException($"Операция '{args[0]}' запрещена."))).Execute();

    private static void RegisterEnqueue(QueueICommand queue) =>
        IoC.Resolve<ICommand>("IoC.Register", GameMessageIocKeys.Enqueue, (IoC.DependencyStrategy)(args =>
        {
            queue.Enqueue((ICommand)args[1]);
            return true;
        })).Execute();

    private static void AssertRejected(Delivery delivery)
    {
        Assert.That(delivery.Acked, Is.False);
        Assert.That(delivery.Nacked, Is.True);
        Assert.That(Response(delivery).Success, Is.False);
    }

    private static GameMessage Message(string json) => JsonSerializer.Deserialize<GameMessage>(json)!;

    private static GameResponse Response(Delivery delivery)
    {
        Assert.That(delivery.PublishedResponse, Is.Not.Null);
        return JsonSerializer.Deserialize<GameResponse>(delivery.PublishedResponse!)!;
    }

    private static string ValidJson(string? token = null, string gameId = "game-1") =>
        JsonSerializer.Serialize(new
        {
            gameId,
            token = token ?? CreateToken("user-1", gameId),
            objectId = "ship-1",
            operationId = "startMove",
            args = new { velocity = 2 },
            timestamp = "2026-06-16T00:00:00Z",
            version = "1.0"
        });

    private static string CreateToken(
        string userId,
        string gameId,
        DateTime? notBefore = null,
        DateTime? expires = null,
        string signingKey = SigningKey)
    {
        DateTime validFrom = notBefore ?? DateTime.UtcNow.AddMinutes(-1);
        DateTime validTo = expires ?? DateTime.UtcNow.AddMinutes(30);
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtGameMessageAuthorizer.GameIdClaim, gameId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        JwtSecurityToken token = new(
            Issuer,
            Audience,
            claims,
            validFrom,
            validTo,
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
