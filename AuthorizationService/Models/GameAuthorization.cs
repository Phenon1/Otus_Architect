namespace AuthorizationService.Models;

public sealed record GameAuthorization(string GameId, IReadOnlySet<string> ParticipantIds);
