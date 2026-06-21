using System.Collections.Concurrent;
using AuthorizationService.Models;

namespace AuthorizationService.Repositories;

public sealed class InMemoryGameAuthorizationRepository : IGameAuthorizationRepository
{
    private readonly ConcurrentDictionary<string, GameAuthorization> _games = new();

    public GameAuthorization Create(IEnumerable<string> participantIds)
    {
        return Create(Guid.NewGuid().ToString(), participantIds);
    }

    public GameAuthorization Create(string gameId, IEnumerable<string> participantIds)
    {
        HashSet<string> participants = participantIds.ToHashSet(StringComparer.Ordinal);
        GameAuthorization game = new(gameId, participants);

        if (!_games.TryAdd(gameId, game))
        {
            throw new InvalidOperationException(
                $"Игра с идентификатором '{gameId}' уже существует.");
        }

        return game;
    }

    public bool TryGet(string gameId, out GameAuthorization game)
    {
        return _games.TryGetValue(gameId, out game!);
    }
}
