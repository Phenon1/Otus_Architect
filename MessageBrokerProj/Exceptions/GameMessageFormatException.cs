using System.Text.Json;

namespace MessageBrokerProj;

public class GameMessageFormatException : JsonException
{
    public GameMessageFormatException(string message)
        : base(message)
    {
    }

    public GameMessageFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
