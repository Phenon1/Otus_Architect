using CommandsProj;
using CommandsProj.CommandExceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBrokerProj;

public sealed class RabbitMqGameMessageEndpoint : IDisposable
{
    public const string DefaultQueueName = "agent.commands";

    private readonly ConnectionFactory? _connectionFactory;
    private readonly IGameRegistry _gameRegistry;
    private readonly IIncomingMessageSerializer _serializer;
    private readonly string _queueName;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqGameMessageEndpoint(
        IGameRegistry gameRegistry,
        IIncomingMessageSerializer serializer,
        string queueName = DefaultQueueName,
        ConnectionFactory? connectionFactory = null)
    {
        _gameRegistry = gameRegistry;
        _serializer = serializer;
        _queueName = queueName;
        _connectionFactory = connectionFactory;
    }

    public void Start()
    {
        if (_connectionFactory == null)
        {
            throw new RabbitMqEndpointConfigurationException();
        }

        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (_, args) => HandleDelivery(new RabbitMqBrokerDelivery(_channel, args));

        _channel.BasicConsume(_queueName, autoAck: false, consumer);
    }

    public void HandleDelivery(IBrokerDelivery delivery)
    {
        try
        {
            GameMessage message = _serializer.Deserialize(delivery.Body);

            if (!_gameRegistry.TryGetGame(message.GameId, out GameContext gameContext))
            {
                Reject(delivery, new GameNotFoundException(message.GameId).Message);
                return;
            }

            gameContext.CommandQueue.Enqueue(new InterpretCommand(message, gameContext));
            SendResponse(delivery, new GameResponse { Success = true });
            delivery.Ack();
        }
        catch (GameMessageFormatException ex)
        {
            Reject(delivery, $"Некорректное сообщение: {ex.Message}");
        }
        catch (QueueAddCommandException ex)
        {
            Reject(delivery, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            Reject(delivery, ex.Message);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }

    private void Reject(IBrokerDelivery delivery, string error)
    {
        SendResponse(delivery, new GameResponse { Success = false, Error = error });
        delivery.Nack(requeue: false);
    }

    private void SendResponse(IBrokerDelivery delivery, GameResponse response)
    {
        if (!string.IsNullOrWhiteSpace(delivery.ReplyTo))
        {
            delivery.PublishResponse(_serializer.SerializeResponse(response));
        }
    }

    private sealed class RabbitMqBrokerDelivery : IBrokerDelivery
    {
        private readonly IModel _channel;
        private readonly BasicDeliverEventArgs _args;

        public RabbitMqBrokerDelivery(IModel channel, BasicDeliverEventArgs args)
        {
            _channel = channel;
            _args = args;
        }

        public ReadOnlyMemory<byte> Body => _args.Body;
        public ulong DeliveryTag => _args.DeliveryTag;
        public string? ReplyTo => _args.BasicProperties?.ReplyTo;
        public string? CorrelationId => _args.BasicProperties?.CorrelationId;

        public void Ack()
        {
            _channel.BasicAck(DeliveryTag, multiple: false);
        }

        public void Nack(bool requeue)
        {
            _channel.BasicNack(DeliveryTag, multiple: false, requeue);
        }

        public void PublishResponse(ReadOnlyMemory<byte> body)
        {
            if (string.IsNullOrWhiteSpace(ReplyTo))
            {
                return;
            }

            IBasicProperties props = _channel.CreateBasicProperties();
            props.CorrelationId = CorrelationId;
            props.ContentType = "application/json";

            _channel.BasicPublish(exchange: string.Empty, routingKey: ReplyTo, basicProperties: props, body: body);
        }
    }
}
