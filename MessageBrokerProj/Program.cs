using CommandsProj;
using IoCProj;
using ModelsProj;
using ModelsProj.TypesObject;
using RabbitMQ.Client;

namespace MessageBrokerProj;

public static class Program
{
    private const string DemoGameId = "game-1";
    private const string DemoObjectId = "ship-1";

    public static void Main()
    {
        QueueICommand gameQueue = new QueueICommand();
        CommandProcessorThread processor = new CommandProcessorThread(gameQueue);
        InMemoryGameRegistry registry = new InMemoryGameRegistry();

        ConfigureDemoGame(gameQueue, registry);

        using RabbitMqGameMessageEndpoint endpoint = new RabbitMqGameMessageEndpoint(
            registry,
            new SystemTextJsonIncomingMessageSerializer(),
            RabbitMqGameMessageEndpoint.DefaultQueueName,
            CreateConnectionFactory());

        processor.Start();
        endpoint.Start();

        Console.WriteLine("Endpoint приема сообщений запущен.");
        Console.WriteLine($"Очередь RabbitMQ: {RabbitMqGameMessageEndpoint.DefaultQueueName}");
        Console.WriteLine($"Демо-игра: {DemoGameId}, демо-объект: {DemoObjectId}, операция: startMove");
        Console.WriteLine("Нажмите Ctrl+C для остановки.");

        using ManualResetEventSlim stopSignal = new ManualResetEventSlim(false);
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            stopSignal.Set();
        };

        stopSignal.Wait();
        processor.HardStop();
    }

    private static ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest",
            VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/",
            Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out int port)
                ? port
                : AmqpTcpEndpoint.UseDefaultPort
        };
    }

    private static void ConfigureDemoGame(QueueICommand queue, InMemoryGameRegistry registry)
    {
        string scopeId = "demo-game-scope";
        SpaceShip ship = new SpaceShip();

        IoC.Resolve<ICommand>("Scopes.Current", "root").Execute();
        IoC.Resolve<ICommand>("Scopes.New", scopeId).Execute();

        IoC.Resolve<ICommand>("IoC.Register", GameMessageIocKeys.Validate, (IoC.DependencyStrategy)(args =>
        {
            GameMessage message = (GameMessage)args[0];

            if (message.GameId != DemoGameId)
            {
                return GameMessageValidationResult.Invalid("Сообщение адресовано неизвестной игре.");
            }

            if (message.ObjectId != DemoObjectId)
            {
                return GameMessageValidationResult.Invalid("Сообщение адресовано неизвестному объекту.");
            }

            return GameMessageValidationResult.Valid();
        })).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Game.Objects", (IoC.DependencyStrategy)(args =>
            (string)args[0] == DemoObjectId
                ? ship
                : throw new KeyNotFoundException($"Объект '{args[0]}' не найден."))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", GameMessageIocKeys.AllowedOperation, (IoC.DependencyStrategy)(args =>
        {
            if ((string)args[0] != "startMove")
            {
                throw new KeyNotFoundException($"Операция '{args[0]}' запрещена.");
            }

            return (Func<GameMessage, ICommand>)CreateStartMoveCommand;
        })).Execute();

        IoC.Resolve<ICommand>("IoC.Register", GameMessageIocKeys.Enqueue, (IoC.DependencyStrategy)(args =>
        {
            if ((string)args[0] != DemoGameId)
            {
                return false;
            }

            queue.Enqueue((ICommand)args[1]);
            return true;
        })).Execute();

        registry.TryRegister(new GameContext(DemoGameId, queue, scopeId));
    }

    private static ICommand CreateStartMoveCommand(GameMessage message)
    {
        if (!message.Args.TryGetProperty("velocity", out var velocity) || !velocity.TryGetInt32(out int value))
        {
            throw new GameMessageSecurityException("Аргумент 'velocity' обязателен и должен быть целым числом.");
        }

        return new ConsoleCommand($"Команда startMove принята для объекта {message.ObjectId}. velocity={value}");
    }

    private sealed class ConsoleCommand : ICommand
    {
        private readonly string _message;

        public ConsoleCommand(string message)
        {
            _message = message;
        }

        public void Execute()
        {
            Console.WriteLine(_message);
        }
    }
}
