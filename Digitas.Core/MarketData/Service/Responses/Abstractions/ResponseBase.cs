using Digitas.Core.Shared;

namespace Digitas.Core.MarketData.Service.Responses;

public abstract record ResponseBase : MessageBase
{
    public string? Channel { get; protected set; }

    public Currency? BaseCurrency { get; protected set; }

    public Currency? QuoteCurrency { get; protected set; }
}
