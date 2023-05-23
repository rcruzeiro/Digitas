namespace Digitas.Core.MarketData.Service.Requests;

public sealed record UnsubscribeRequest(ChannelBase Channel) : RequestBase(Channel)
{
    public override string Event => "bts:unsubscribe";
}
