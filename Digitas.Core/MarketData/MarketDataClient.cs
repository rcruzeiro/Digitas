using Digitas.Core.MarketData.Service;
using Digitas.Core.MarketData.Service.Requests;
using Digitas.Core.MarketData.Service.Responses;
using Digitas.Core.Shared;

namespace Digitas.Core.MarketData;

public sealed class MarketDataClient : IMarketDataClient
{
    private readonly Uri url = BitstampValues.WebsocketUrl;

    public async Task GetOrderBookAsync(Func<OrderBookResponse, ValueTask> func, params Currency[] currencies)
    {
        using IBitstampWebsocketClient socket = new BitstampWebsocketClient(url);
        socket.Name = Guid.NewGuid().ToString();

        using IBitstampService client = new BitstampService(socket);

        socket.ReconnectionHappened.Subscribe(async t =>
        {
            await SendRequests();
        });

        await socket.Start();
        await WriteOrderBook();

        async ValueTask WriteOrderBook()
        {
            await foreach (var item in BitstampStreams.OrderBook.ReadAllAsync())
            {
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
}
