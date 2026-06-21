using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationService.Security;

public sealed class JwtGameTokenService : IGameTokenService
{
    public const string GameIdClaim = "game_id";

    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _signingCredentials;

    public JwtGameTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public GameToken Create(string userId, string gameId)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddMinutes(_options.LifetimeMinutes);
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId),
            new(GameIdClaim, gameId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now.UtcDateTime).ToString(), ClaimValueTypes.Integer64)
        ];

        JwtSecurityToken token = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: _signingCredentials);

        return new GameToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
