namespace AuthorizationService.Contracts;

public sealed record CreateGameTokenResponse(string Token, DateTimeOffset ExpiresAt);
