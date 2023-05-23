namespace Digitas.Core.MarketData.Service.Requests;

public sealed record SubscribeRequest(ChannelBase Channel) : RequestBase(Channel)
{
    public override string Event => "bts:subscribe";
}
