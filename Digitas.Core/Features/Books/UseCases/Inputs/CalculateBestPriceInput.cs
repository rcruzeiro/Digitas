using Digitas.Core.Shared;
using Digitas.Core.Shared.Mediator;

namespace Digitas.Core.Features.Books.UseCases.Inputs;

public sealed record CalculateBestPriceInput(Currency BaseCurrency, OrderSide Side, double Quantity) : IInput
{
    public Currency QuoteCurrency { get; init; } = Currency.USD;
}
