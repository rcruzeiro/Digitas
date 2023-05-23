using Digitas.Core.Shared;
using Digitas.MarketData.Bitstamp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Digitas.Core.MarketData.Service.Responses;

public sealed record OrderBookResponse : ResponseBase
{
    public override MessageType Event => MessageType.OrderBook;

    public OrderBook? Data { get; set; }

    internal static bool TryHandle(JObject response)
    {
        if (response is null) return false;

        var channelName = response["channel"];

        if (channelName is not null
         && !string.IsNullOrEmpty(channelName.Value<string>())
         && channelName.Value<string>()!.StartsWith("order_book"))
        {
            var parsed = response.ToObject<OrderBookResponse>(BitstampExtensions.Serializer);

            if (parsed is not null)
            {
                parsed.Channel = channelName.Value<string>();

                var symbol = BitstampExtensions.BrokePair(parsed.Channel?.Split('_').LastOrDefault() ?? string.Empty);

                parsed.BaseCurrency = symbol.BaseCurrency;
                parsed.QuoteCurrency = symbol.QuoteCurrency;

                // write into stream channel
                BitstampStreams.OrderBookChannel.Writer.TryWrite(parsed);

                return true;
            }
        }

        return false;
    }
}

#region order book components
public sealed record OrderBook
{
    [JsonConverter(typeof(OrderBookDataConverter), OrderSide.Bid)]
    public OrderBookData[]? Bids { get; set; }

    [JsonConverter(typeof(OrderBookDataConverter), OrderSide.Ask)]
    public OrderBookData[]? Asks { get; set; }
}

public sealed record OrderBookData
{
    public long? OrderId { get; set; }

    public OrderSide Side { get; set; }

    public double Price { get; set; }

    public double Amount { get; set; }
}

class OrderBookDataConverter : JsonConverter
{
    private readonly OrderSide _side;

    public OrderBookDataConverter()
    { }

    public OrderBookDataConverter(OrderSide side)
        => _side = side;

    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType)
        => objectType == typeof(double[]);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var arr = JArray.Load(reader);

        return FromJArray(arr);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        => throw new NotImplementedException();

    private OrderBookData[] FromJArray(JArray data)
    {
        var result = new List<OrderBookData>();

        foreach (var item in data)
        {
            var arr = item.ToArray();
            var bd = new OrderBookData();

            if (arr.Length == 2)
            {
                bd.Side = _side;
                bd.Price = (double)arr[0];
                bd.Amount = (double)arr[1];
            }
            else
            {
                bd.Side = _side;
                bd.Price = (double)arr[0];
                bd.Amount = (double)arr[1];
                bd.OrderId = (long)arr[2];
            }

            result.Add(bd);
        }

        return result.ToArray();
    }
}
#endregion
