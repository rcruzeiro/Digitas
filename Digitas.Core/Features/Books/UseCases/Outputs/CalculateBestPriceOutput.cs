using Digitas.Core.Data.Models;
using Digitas.Core.Shared;

namespace Digitas.Core.Features.Books.UseCases.Outputs;

public sealed record CalculateBestPriceOutput
{
    public string? Id { get; set; }

    public Currency Currency { get; init; }

    public string? Symbol { get; init; }

    public OrderSide Side { get; init; }

    public double RequestedQuantity { get; init; }

    public double ServedQuantity { get; set; }

    public IEnumerable<OrderBookData>? OrdersBook { get; set; }

    public double? Total { get; set; }
}
