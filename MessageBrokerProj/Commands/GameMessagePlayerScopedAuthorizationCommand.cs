using System.IdentityModel.Tokens.Jwt;
using CommandsProj;

namespace MessageBrokerProj;

public sealed class GameMessagePlayerScopedAuthorizationCommand : ICommand
{
    private readonly GameMessage _message;
    private readonly JwtSecurityTokenHandler _handler = new()
    {
        MapInboundClaims = false
    };

    public GameMessagePlayerScopedAuthorizationCommand(GameMessage message)
    {
        _message = message;
    }

    public void Execute()
    {
        string playerId = GetPlayerId();

        PlayerScopedObjectOwnership.EnsureOwned(
            playerId,
            _message.ObjectId,
            GameMessageIocKeys.PlayerScope,
            OrderIocKeys.Objects);
    }

    private string GetPlayerId()
    {
        try
        {
            JwtSecurityToken token = _handler.ReadJwtToken(_message.Token);
            string? playerId = token.Claims.FirstOrDefault(claim =>
                claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (!string.IsNullOrWhiteSpace(playerId))
            {
                return playerId;
            }
        }
        catch (ArgumentException ex)
        {
            throw new GameMessageSecurityException("Не удалось прочитать токен сообщения.", ex);
        }

        throw new GameMessageSecurityException("Токен сообщения не содержит идентификатор игрока.");
    }
}
