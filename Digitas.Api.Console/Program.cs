using Digitas.Core.MarketData;
using Digitas.Core.MarketData.Service;
using Digitas.Core.MarketData.Service.Requests;
using Digitas.Core.Shared;

internal class Program
{
    private static readonly ManualResetEvent ExitEvent = new(false);
    private static readonly CancellationTokenSource cts = new CancellationTokenSource();

    static async Task Main(string[] args)
    {
        Console.CancelKeyPress += Console_CancelKeyPress;

        Console.WriteLine("|=======================|");
        Console.WriteLine("|    BITSTAMP CLIENT    |");
        Console.WriteLine("|=======================|");
        Console.WriteLine();

        var url = BitstampValues.WebsocketUrl;

        using var socket = new BitstampWebsocketClient(url);
        socket.Name = "Bitstamp Console Edition";

        using var client = new BitstampService(socket);

        socket.ReconnectionHappened.Subscribe(async t =>
        {
            await SendRequests(client);
        });

        await socket.Start();
        await WriteOrderBook();

        ExitEvent.WaitOne();

        async ValueTask WriteOrderBook()
        {
            await foreach (var item in BitstampStreams.OrderBook.ReadAllAsync(cts.Token).WithCancellation(cts.Token))
            {
                if (item.Data is not null && item.Data.Bids!.Any())
                {
                    for (var i = 0; i < item.Data.Bids?.Length; i++)
                    {
                        Console.WriteLine($"{item.Data.Bids?[i].Amount ?? 0} {item.BaseCurrency} @ {item.Data.Bids?[i].Price ?? 0} {item.QuoteCurrency}");
                    }
                }
            }
        }
    }

    private static Task SendRequests(BitstampService client)
    {
        var btc = new OrderBookChannel(Currency.BTC);
        var eth = new OrderBookChannel(Currency.ETH);

        client.Send(new SubscribeRequest(btc));
        client.Send(new SubscribeRequest(eth));

        return Task.CompletedTask;
    }

    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Cancelling process..");
        e.Cancel = true;
        cts.Cancel();
        ExitEvent.Set();
    }
}
