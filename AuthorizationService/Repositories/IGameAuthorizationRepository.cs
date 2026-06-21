using AuthorizationService.Models;

namespace AuthorizationService.Repositories;

public interface IGameAuthorizationRepository
{
    GameAuthorization Create(IEnumerable<string> participantIds);
    GameAuthorization Create(string gameId, IEnumerable<string> participantIds);
    bool TryGet(string gameId, out GameAuthorization game);
}
