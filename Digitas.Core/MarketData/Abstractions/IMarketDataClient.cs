using Digitas.Core.MarketData.Service.Responses;
using Digitas.Core.Shared;

namespace Digitas.Core.MarketData;

public interface IMarketDataClient
{
    Task GetOrderBookAsync(Func<OrderBookResponse, ValueTask> func, params Currency[] currencies);
}
