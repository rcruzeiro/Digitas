using Digitas.Core.Shared;
using Digitas.MarketData.Bitstamp;

namespace Digitas.Core.MarketData.Service.Requests;

public abstract record ChannelBase(Currency BaseCurrency)
{
    public abstract string Name { get; }

    public abstract string Type { get; }

    public Currency QuoteCurrency { get; init; } = Currency.USD;

    public string FormatToBitstamp()
    {
        return string.Join('_', Type.ToLower(), BaseCurrency.PairWith(QuoteCurrency));
    }
}
