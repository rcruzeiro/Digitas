using Digitas.MarketData.Channels;

namespace Digitas.MarketData.Requests.Policies;

public sealed record UnsubscribeRequest(ChannelBase Channel) : RequestBase(Channel)
{
    public override string Event => "bts:unsubscribe";

    public override RequestData Data => new() { Channel = GetChannelStructure() };

    private string GetChannelStructure()
    {
        return Channel.ToString();
    }
}
