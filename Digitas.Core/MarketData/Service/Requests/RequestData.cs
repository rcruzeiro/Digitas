using Newtonsoft.Json;

namespace Digitas.Core.MarketData.Service.Requests;

public sealed record RequestData
{
    [JsonProperty("channel")]
    public string? Channel { get; init; }
}
