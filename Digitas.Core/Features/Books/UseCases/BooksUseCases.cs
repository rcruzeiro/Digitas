using Digitas.Core.Data.Models;
using Digitas.Core.Data.MongoDb.Abstractions;
using Digitas.Core.Features.Books.UseCases.Inputs;
using Digitas.Core.Features.Books.UseCases.Outputs;
using Digitas.Core.Shared;
using Digitas.Core.Shared.Mediator;
using LanguageExt.Common;

namespace Digitas.Core.Features.Books.UseCases;

public sealed class BooksUseCases : IUseCase,
    IUseCaseHandler<CreateOrderBookInput>,
    IUseCaseHandler<CalculateBestPriceInput, CalculateBestPriceOutput>
{
    private readonly IMongoDbRepository _repository;

    public BooksUseCases(IMongoDbRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(CreateOrderBookInput input, CancellationToken cancellationToken = default)
    {
        if (input is null) return;

        // create order book into database
        await _repository.CreateAsync(input.Book, cancellationToken);
    }

    public async Task<Result<CalculateBestPriceOutput>> HandleAsync(CalculateBestPriceInput input, CancellationToken cancellationToken = default)
    {
        if (input is null)
        {
            return new Result<CalculateBestPriceOutput>(new ArgumentNullException(nameof(input)));
        }

        var output = new CalculateBestPriceOutput
        {
            Currency = input.BaseCurrency,
            Side = input.Side,
            Symbol = $"{input.BaseCurrency}{input.QuoteCurrency}".ToLower(),
            RequestedQuantity = input.Quantity
        };

        try
        {
            // get book from database w/ queryable linq
            var data = (from ob in await _repository.GetAsQueryableAsync<OrderBook>()
                        where ob.Symbol == output.Symbol
                        select ob).ToList();

            if (!data.Any())
            {
                // return exception as result
                return new Result<CalculateBestPriceOutput>(
                    new InvalidOperationException($"Order book not found for {input.Side.ToString().ToLower()} in {output.Symbol}."));
            }

            // get first returned book (if more than one)
            var book = data.Last();
            double amount = 0;

            if (input.Side == OrderSide.Ask) // buy
            {
                var collection = output.OrdersBook = ProcessAsks();

                output.ServedQuantity = collection?.Sum(_ => _.Amount) ?? 0;
                output.Total = collection?.Sum(_ => _.Price) ?? 0;
            }
            else // sell
            {
                var collection = output.OrdersBook = ProcessBids();

                output.ServedQuantity = collection?.Sum(_ => _.Amount) ?? 0;
                output.Total = collection?.Sum(_ => _.Price) ?? 0;
            }

            // update output with order book id
            output.Id = book.Id.ToString();

            return new Result<CalculateBestPriceOutput>(output);

            IEnumerable<OrderBookData>? ProcessAsks()
            {
                return book.Asks?
                    .OrderBy(_ => _.Price)
                    .TakeWhile(item => (amount += item.Amount) <= input.Quantity).ToList();
            }

            IEnumerable<OrderBookData>? ProcessBids()
            {
                return book.Bids?
                    .OrderBy(_ => _.Price)
                    .TakeWhile(item => (amount += item.Amount) <= input.Quantity).ToList();
            }
        }
        catch (Exception ex)
        {
            return new Result<CalculateBestPriceOutput>(ex);
        }
    }
}
