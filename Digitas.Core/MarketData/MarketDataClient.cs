using Digitas.Core.MarketData.Service;
using Digitas.Core.MarketData.Service.Requests;
using Digitas.Core.MarketData.Service.Responses;
using Digitas.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Digitas.Core.MarketData;

public sealed class MarketDataClient : IMarketDataClient
{
    private readonly ILogger<MarketDataClient> _logger;
    private readonly Uri url = BitstampValues.WebsocketUrl;

    public MarketDataClient(ILogger<MarketDataClient> logger)
    {
        _logger = logger;
    }

    public async Task GetOrderBookAsync(Func<OrderBookResponse, ValueTask> func, params Currency[] currencies)
    {
        try
        {
            _logger.LogInformation("Getting order book from Bitstamp market data.");

            using IBitstampWebsocketClient socket = new BitstampWebsocketClient(url);
            socket.Name = Guid.NewGuid().ToString();

            using IBitstampService client = new BitstampService(socket);

            _logger.LogInformation("Subscribing currencies..");

            socket.ReconnectionHappened.Subscribe(async t =>
            {
                await SendRequests();
            });

            _logger.LogInformation("Starting socket..");

            await socket.Start();
            await WriteOrderBook();

            async ValueTask WriteOrderBook()
            {
                await foreach (var item in BitstampStreams.OrderBook.ReadAllAsync())
                {
                    _logger.LogInformation("Incoming new message from market data..");

                    await func(item);
                }
            }

            Task SendRequests()
            {
                foreach (var currency in currencies)
                {
                    var channel = new OrderBookChannel(currency);
                    client.Send(new SubscribeRequest(channel));
                }

                return Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{message}", ex.Message);
        }
    }
}
