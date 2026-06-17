namespace MessageBrokerProj;

public interface IIncomingMessageSerializer
{
    GameMessage Deserialize(ReadOnlyMemory<byte> body);
    byte[] SerializeResponse(GameResponse response);
}
