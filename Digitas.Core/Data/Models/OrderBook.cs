using Digitas.Core.MarketData.Service.Responses;
using MongoDB.Bson;

namespace Digitas.Core.Data.Models;

public sealed record OrderBook : MongoModelBase
{
    public override string CollectionName => "OrderBook";

    public string? Symbol { get; set; }

    public OrderBookData[]? Bids { get; set; }

    public OrderBookData[]? Asks { get; set; }

    public static OrderBook CreateFromMarketData(OrderBookResponse book)
    {
        if (book is null) throw new ArgumentNullException(nameof(book));

        if (book.Data is null
         || book.Data.Asks is null
         || !book.Data.Asks.Any()
         || book.Data.Bids is null
         || !book.Data.Bids.Any()) throw new ArgumentException("Invalid book.");

        List<OrderBookData> asks = new();
        List<OrderBookData> bids = new();

        // asks
        foreach (var item in book.Data.Asks)
        {
            asks.Add(new()
            {
                Amount = item.Amount,
                OrderId = item.OrderId,
                Price = item.Price,
                Side = (int)item.Side
            });
        }

        // bids
        foreach (var item in book.Data.Bids)
        {
            bids.Add(new()
            {
                Amount = item.Amount,
                OrderId = item.OrderId,
                Price = item.Price,
                Side = (int)item.Side
            });
        }

        return new()
        {
            Id = ObjectId.GenerateNewId(),
            Symbol = $"{book.BaseCurrency}{book.QuoteCurrency}".ToLower(),
            Asks = asks.ToArray(),
            Bids = bids.ToArray()
        };
    }
}

public sealed record OrderBookData
{
    public long? OrderId { get; set; }

    public int Side { get; set; }

    public double Price { get; set; }

    public double Amount { get; set; }
}
