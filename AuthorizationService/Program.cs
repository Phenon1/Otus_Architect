using System.Text;
using AuthorizationService.Contracts;
using AuthorizationService.Models;
using AuthorizationService.Repositories;
using AuthorizationService.Security;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? legacySigningKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");
string? legacyIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
string? legacyAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

if (!string.IsNullOrWhiteSpace(legacySigningKey))
{
    builder.Configuration["Jwt:SigningKey"] = legacySigningKey;
}

if (!string.IsNullOrWhiteSpace(legacyIssuer))
{
    builder.Configuration["Jwt:Issuer"] = legacyIssuer;
}

if (!string.IsNullOrWhiteSpace(legacyAudience))
{
    builder.Configuration["Jwt:Audience"] = legacyAudience;
}

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey),
        "Не задан ключ подписи JWT. Настройте Jwt:SigningKey или JWT_SIGNING_KEY.")
    .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
        "Ключ подписи JWT должен содержать не менее 32 байт в кодировке UTF-8.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer),
        "Не задан издатель JWT (issuer).")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience),
        "Не задан получатель JWT (audience).")
    .Validate(options => options.LifetimeMinutes > 0,
        "Срок действия JWT должен быть больше нуля.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IGameAuthorizationRepository, InMemoryGameAuthorizationRepository>();
builder.Services.AddSingleton<IGameTokenService, JwtGameTokenService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    string? demoGameId = app.Configuration["DemoGame:GameId"];
    string[] demoParticipantIds =
        app.Configuration.GetSection("DemoGame:ParticipantIds").Get<string[]>() ?? [];

    if (!string.IsNullOrWhiteSpace(demoGameId) && demoParticipantIds.Length > 0)
    {
        IGameAuthorizationRepository repository =
            app.Services.GetRequiredService<IGameAuthorizationRepository>();

        if (!repository.TryGet(demoGameId, out _))
        {
            repository.Create(demoGameId, demoParticipantIds);
        }
    }
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Сервис авторизации игр",
    createGame = "POST /api/games",
    createToken = "POST /api/games/{gameId}/tokens",
    demoGame = "game-1",
    demoUsers = new[] { "user-1", "user-2" }
}));

app.MapPost("/api/games", (
    CreateGameRequest request,
    IGameAuthorizationRepository repository) =>
{
    if (request.ParticipantIds is null || request.ParticipantIds.Count == 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["participantIds"] = ["Необходимо указать хотя бы одного участника."]
        });
    }

    if (request.ParticipantIds.Any(string.IsNullOrWhiteSpace))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["participantIds"] = ["Идентификаторы участников не должны быть пустыми."]
        });
    }

    string[] participantIds = request.ParticipantIds
        .Select(id => id.Trim())
        .Distinct(StringComparer.Ordinal)
        .ToArray();

    GameAuthorization game = repository.Create(participantIds);
    return Results.Created($"/api/games/{game.GameId}", new CreateGameResponse(game.GameId));
});

app.MapPost("/api/games/{gameId}/tokens", (
    string gameId,
    CreateGameTokenRequest request,
    IGameAuthorizationRepository repository,
    IGameTokenService tokenService) =>
{
    if (string.IsNullOrWhiteSpace(request.UserId))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["userId"] = ["Идентификатор пользователя обязателен."]
        });
    }

    if (!repository.TryGet(gameId, out GameAuthorization game))
    {
        return Results.Problem(
            detail: $"Игра '{gameId}' не найдена.",
            statusCode: StatusCodes.Status404NotFound);
    }

    string userId = request.UserId.Trim();
    if (!game.ParticipantIds.Contains(userId))
    {
        return Results.Problem(
            detail: $"Пользователь '{userId}' не является участником игры '{gameId}'.",
            statusCode: StatusCodes.Status403Forbidden);
    }

    GameToken token = tokenService.Create(userId, game.GameId);
    return Results.Ok(new CreateGameTokenResponse(token.Value, token.ExpiresAt));
});

app.Run();

public partial class Program;
