namespace Digitas.Core.MarketData.Service.Responses;

public abstract record MessageBase
{
    public abstract MessageType Event { get; }
}
