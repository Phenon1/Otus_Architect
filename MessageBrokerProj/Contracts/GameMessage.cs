using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageBrokerProj;

public class GameMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("objectId")]
    public string ObjectId { get; set; } = string.Empty;

    [JsonPropertyName("operationId")]
    public string OperationId { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public JsonElement Args { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";
}
