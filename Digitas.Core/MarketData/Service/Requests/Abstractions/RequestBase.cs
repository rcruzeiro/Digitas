using Newtonsoft.Json;

namespace Digitas.Core.MarketData.Service.Requests;

public abstract record RequestBase(ChannelBase Channel)
{
    [JsonProperty("event")]
    public abstract string Event { get; }

    [JsonProperty("data")]
    public virtual RequestData Data => new() { Channel = FormatChannel() };

    protected string FormatChannel()
    {
        return Channel.FormatToBitstamp();
    }
}
