using System.Text.Json.Serialization;
using Digitas.MarketData.Channels;

namespace Digitas.MarketData.Requests;

public abstract record RequestBase(ChannelBase Channel)
{
    [JsonPropertyName("event")]
    public abstract string Event { get; }

    [JsonPropertyName("data")]
    public abstract RequestData Data { get; }
}
