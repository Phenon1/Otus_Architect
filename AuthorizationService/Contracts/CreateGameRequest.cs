namespace AuthorizationService.Contracts;

public sealed record CreateGameRequest(IReadOnlyCollection<string>? ParticipantIds);
