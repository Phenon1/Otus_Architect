namespace AuthorizationService.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "otus-authorization-service";
    public string Audience { get; set; } = "otus-game-server";
    public int LifetimeMinutes { get; set; } = 30;
}
