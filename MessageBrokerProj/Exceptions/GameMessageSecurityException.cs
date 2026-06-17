using System.Security;

namespace MessageBrokerProj;

public class GameMessageSecurityException : SecurityException
{
    public GameMessageSecurityException(string message)
        : base(message)
    {
    }

    public GameMessageSecurityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
