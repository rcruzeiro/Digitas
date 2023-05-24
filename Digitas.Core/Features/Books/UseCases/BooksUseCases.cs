using Digitas.Core.Data.Models;
using Digitas.Core.Data.MongoDb.Abstractions;
using Digitas.Core.Features.Books.UseCases.Inputs;
using Digitas.Core.Features.Books.UseCases.Outputs;
using Digitas.Core.Shared;
using Digitas.Core.Shared.Mediator;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;

namespace Digitas.Core.Features.Books.UseCases;

public sealed class BooksUseCases : IUseCase,
    IUseCaseHandler<CreateOrderBookInput>,
    IUseCaseHandler<CalculateBestPriceInput, CalculateBestPriceOutput>
{
    private readonly ILogger<BooksUseCases> _logger;
    private readonly IMongoDbRepository _repository;

    public BooksUseCases(ILogger<BooksUseCases> logger, IMongoDbRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task HandleAsync(CreateOrderBookInput input, CancellationToken cancellationToken = default)
    {
        if (input is null)
        {
            _logger.LogWarning("Invalid input to handle.");

            return;
        }

        _logger.LogInformation("Handling OrderBook creation..");

        // create order book into database
        await _repository.CreateAsync(input.Book, cancellationToken);
    }

    public async Task<Result<CalculateBestPriceOutput>> HandleAsync(CalculateBestPriceInput input, CancellationToken cancellationToken = default)
    {
        if (input is null)
        {
            _logger.LogWarning("Invalid input to handle.");

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
            _logger.LogInformation("Obtaining information from database for {currency} and a quantity of {quantity}..",
                                   input.BaseCurrency,
                                   input.Quantity);

            // get book from database w/ queryable linq
            var data = (from ob in await _repository.GetAsQueryableAsync<OrderBook>()
                        where ob.Symbol == output.Symbol
                        select ob).ToList();

            if (!data.Any())
            {
                _logger.LogWarning("No information was found.");

                // return exception as result
                return new Result<CalculateBestPriceOutput>(
                    new InvalidOperationException($"Order book not found for {input.Side.ToString().ToLower()} in {output.Symbol}."));
            }

            // get first returned book (if more than one)
            var book = data.Last();
            double amount = 0;

            if (input.Side == OrderSide.Ask) // buy
            {
                _logger.LogInformation("Processing ASK information..");

                var collection = output.OrdersBook = ProcessAsks();

                output.ServedQuantity = collection?.Sum(_ => _.Amount) ?? 0;
                output.Total = collection?.Sum(_ => _.Price) ?? 0;
            }
            else // sell
            {
                _logger.LogInformation("Processing BID information..");

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
            _logger.LogError(ex, "{message}", ex.Message);

            return new Result<CalculateBestPriceOutput>(ex);
        }
    }
}
