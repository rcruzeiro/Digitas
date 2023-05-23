using System.Text.Json.Serialization;

namespace Digitas.MarketData.Requests;

public sealed record RequestData
{
    [JsonPropertyName("channel")]
    public string? Channel { get; init; }
}
