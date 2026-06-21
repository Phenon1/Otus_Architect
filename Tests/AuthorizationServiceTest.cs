using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using AuthorizationService.Contracts;
using AuthorizationService.Models;
using AuthorizationService.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Tests;

public class AuthorizationServiceTest
{
    private const string SigningKey = "authorization-service-test-key-32-bytes-minimum";
    private const string Issuer = "test-authorization-service";
    private const string Audience = "test-game-server";

    [Test]
    public async Task CreateGameReturnsIdAndStoresDistinctParticipants()
    {
        using AuthorizationServiceFactory factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/games",
            new CreateGameRequest(["user-1", "user-2", "user-1"]));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        CreateGameResponse? result = await response.Content.ReadFromJsonAsync<CreateGameResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(Guid.TryParse(result!.GameId, out _), Is.True);

        IGameAuthorizationRepository repository =
            factory.Services.GetRequiredService<IGameAuthorizationRepository>();
        Assert.That(repository.TryGet(result.GameId, out GameAuthorization game), Is.True);
        Assert.That(game.ParticipantIds, Is.EquivalentTo(new[] { "user-1", "user-2" }));
    }

    [TestCaseSource(nameof(InvalidParticipants))]
    public async Task CreateGameRejectsInvalidParticipants(CreateGameRequest request)
    {
        using AuthorizationServiceFactory factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/games", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ParticipantReceivesCryptographicallyValidGameToken()
    {
        using AuthorizationServiceFactory factory = new();
        using HttpClient client = factory.CreateClient();
        string gameId = await CreateGame(client, "user-1", "user-2");

        HttpResponseMessage response = await client.PostAsJsonAsync(
            $"/api/games/{gameId}/tokens",
            new CreateGameTokenRequest("user-1"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        CreateGameTokenResponse? result =
            await response.Content.ReadFromJsonAsync<CreateGameTokenResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExpiresAt, Is.GreaterThan(DateTimeOffset.UtcNow));

        JwtSecurityTokenHandler handler = new() { MapInboundClaims = false };
        var principal = handler.ValidateToken(result.Token, ValidationParameters(), out _);

        Assert.That(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, Is.EqualTo("user-1"));
        Assert.That(principal.FindFirst("game_id")?.Value, Is.EqualTo(gameId));
        Assert.That(principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value, Is.Not.Empty);
        Assert.That(principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value, Is.Not.Empty);
    }

    [Test]
    public async Task TokenEndpointRejectsNonParticipantAndUnknownGame()
    {
        using AuthorizationServiceFactory factory = new();
        using HttpClient client = factory.CreateClient();
        string gameId = await CreateGame(client, "user-1");

        HttpResponseMessage forbidden = await client.PostAsJsonAsync(
            $"/api/games/{gameId}/tokens",
            new CreateGameTokenRequest("intruder"));
        HttpResponseMessage notFound = await client.PostAsJsonAsync(
            $"/api/games/{Guid.NewGuid()}/tokens",
            new CreateGameTokenRequest("user-1"));

        Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(notFound.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private static IEnumerable<CreateGameRequest> InvalidParticipants()
    {
        yield return new CreateGameRequest(null);
        yield return new CreateGameRequest([]);
        yield return new CreateGameRequest(["user-1", " "]);
    }

    private static async Task<string> CreateGame(HttpClient client, params string[] participants)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/games",
            new CreateGameRequest(participants));
        response.EnsureSuccessStatusCode();

        CreateGameResponse result =
            (await response.Content.ReadFromJsonAsync<CreateGameResponse>())!;
        return result.GameId;
    }

    private static TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
    };

    private sealed class AuthorizationServiceFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("Jwt:SigningKey", SigningKey);
            builder.UseSetting("Jwt:Issuer", Issuer);
            builder.UseSetting("Jwt:Audience", Audience);
            builder.UseSetting("Jwt:LifetimeMinutes", "30");
        }
    }
}
