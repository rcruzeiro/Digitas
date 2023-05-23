using Digitas.Core.Shared;

namespace Digitas.Core.MarketData.Service.Requests;

public sealed record OrderBookChannel(Currency BaseCurrency) : ChannelBase(BaseCurrency)
{
    public override string Name => "Live Order Book";

    public override string Type => "order_book";
}
