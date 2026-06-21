using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MessageBrokerProj;

public sealed class JwtGameMessageAuthorizer : IGameMessageAuthorizer
{
    public const string GameIdClaim = "game_id";

    private readonly JwtSecurityTokenHandler _handler = new()
    {
        MapInboundClaims = false
    };

    private readonly TokenValidationParameters _validationParameters;

    public JwtGameMessageAuthorizer(JwtGameMessageAuthorizerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            throw new ArgumentException("Не задан ключ подписи JWT.", nameof(options));
        }

        if (Encoding.UTF8.GetByteCount(options.SigningKey) < 32)
        {
            throw new ArgumentException(
                "Ключ подписи JWT должен содержать не менее 32 байт в кодировке UTF-8.",
                nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new ArgumentException("Не задан издатель JWT (issuer).", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new ArgumentException("Не задан получатель JWT (audience).", nameof(options));
        }

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
    }

    public GameMessageValidationResult Authorize(GameMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Token))
        {
            return GameMessageValidationResult.Invalid("Необходимо передать JWT.");
        }

        try
        {
            var principal = _handler.ValidateToken(message.Token, _validationParameters, out _);
            string? userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            string? gameId = principal.FindFirst(GameIdClaim)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return GameMessageValidationResult.Invalid(
                    "JWT не содержит идентификатор пользователя.");
            }

            if (!string.Equals(gameId, message.GameId, StringComparison.Ordinal))
            {
                return GameMessageValidationResult.Invalid(
                    "JWT не предоставляет доступ к указанной игре.");
            }

            return GameMessageValidationResult.Valid();
        }
        catch (SecurityTokenExpiredException)
        {
            return GameMessageValidationResult.Invalid("Срок действия JWT истёк.");
        }
        catch (SecurityTokenException)
        {
            return GameMessageValidationResult.Invalid("JWT недействителен.");
        }
        catch (ArgumentException)
        {
            return GameMessageValidationResult.Invalid("JWT недействителен.");
        }
    }
}
