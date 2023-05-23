using System.Threading.Channels;
using Digitas.Core.MarketData.Service.Responses;

namespace Digitas.Core.MarketData.Service;

public sealed class BitstampStreams
{
    internal static readonly Channel<OrderBookResponse> OrderBookChannel = Channel.CreateUnbounded<OrderBookResponse>(new UnboundedChannelOptions()
    {
        SingleWriter = true,
        SingleReader = true
    });

    public static ChannelReader<OrderBookResponse> OrderBook => OrderBookChannel.Reader;
}
