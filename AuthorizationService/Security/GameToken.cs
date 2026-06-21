namespace AuthorizationService.Security;

public sealed record GameToken(string Value, DateTimeOffset ExpiresAt);
