namespace MessageBrokerProj;

public sealed class GameMessageValidationResult
{
    private GameMessageValidationResult(bool isValid, string? error)
    {
        IsValid = isValid;
        Error = error;
    }

    public bool IsValid { get; }
    public string? Error { get; }

    public static GameMessageValidationResult Valid()
    {
        return new GameMessageValidationResult(true, null);
    }

    public static GameMessageValidationResult Invalid(string error)
    {
        return new GameMessageValidationResult(false, error);
    }
}
