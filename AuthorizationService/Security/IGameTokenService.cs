namespace AuthorizationService.Security;

public interface IGameTokenService
{
    GameToken Create(string userId, string gameId);
}
