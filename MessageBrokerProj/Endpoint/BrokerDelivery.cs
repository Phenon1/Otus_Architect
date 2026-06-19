namespace MessageBrokerProj;

public interface IBrokerDelivery
{
    ReadOnlyMemory<byte> Body { get; }
    ulong DeliveryTag { get; }
    string? ReplyTo { get; }
    string? CorrelationId { get; }

    void Ack();
    void Nack(bool requeue);
    void PublishResponse(ReadOnlyMemory<byte> body);
}
