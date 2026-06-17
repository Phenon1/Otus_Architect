using System.Text.Json;

namespace MessageBrokerProj;

public sealed class SystemTextJsonIncomingMessageSerializer : IIncomingMessageSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GameMessage Deserialize(ReadOnlyMemory<byte> body)
    {
        GameMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<GameMessage>(body.Span, _options);
        }
        catch (JsonException ex)
        {
            throw new GameMessageFormatException("Тело сообщения не является корректным JSON.", ex);
        }

        if (message == null)
        {
            throw new GameMessageFormatException("Тело сообщения пустое.");
        }

        if (string.IsNullOrWhiteSpace(message.GameId))
        {
            throw new GameMessageFormatException("Поле сообщения 'gameId' обязательно.");
        }

        if (string.IsNullOrWhiteSpace(message.ObjectId))
        {
            throw new GameMessageFormatException("Поле сообщения 'objectId' обязательно.");
        }

        if (string.IsNullOrWhiteSpace(message.OperationId))
        {
            throw new GameMessageFormatException("Поле сообщения 'operationId' обязательно.");
        }

        return message;
    }

    public byte[] SerializeResponse(GameResponse response)
    {
        return JsonSerializer.SerializeToUtf8Bytes(response, _options);
    }
}
