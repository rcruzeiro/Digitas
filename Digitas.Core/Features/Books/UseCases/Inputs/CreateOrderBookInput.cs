using Digitas.Core.Data.Models;
using Digitas.Core.Shared.Mediator;

namespace Digitas.Core.Features.Books.UseCases.Inputs;

public sealed record CreateOrderBookInput(OrderBook Book) : IInput
{ }
