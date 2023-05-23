using System.Text.Json;
using Digitas.Api.Web.Services;
using Digitas.Core.Features.Books.UseCases;
using Digitas.Core.Features.Books.UseCases.Inputs;
using Digitas.Core.MarketData;
using Digitas.Core.MarketData.Service.Responses;
using Digitas.Core.Shared;
using Digitas.Core.Shared.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Digitas.Api.Web.Controllers.V1;

[ApiVersion("1")]
[ApiController, Produces("application/json")]
[Route("v{version:apiVersion}/books")]
public sealed class BooksController : Controller
{
    private readonly IUseCaseMediator _mediator;
    private readonly IMarketDataClient _marketDataClient;

    public BooksController(IUseCaseMediator mediator, IMarketDataClient marketDataClient)
    {
        _mediator = mediator;
        _marketDataClient = marketDataClient;
    }

    [HttpGet("live")]
    public async Task<IActionResult> GetOrderBookAsync([FromQuery] Currency[] currency, CancellationToken cancellationToken = default)
    {
        var response = Response;

        try
        {
            SseService.ConfigureHeaders(response.Headers);

            // start order book stream
            await _marketDataClient.GetOrderBookAsync(ProcessPrice, currency);

            async ValueTask ProcessPrice(OrderBookResponse book)
            {
                try
                {
                    var data = JsonSerializer.Serialize(book);

                    await SseService.WriteMessageAsync(response, data, cancellationToken);

                    // save order book into database
                    await UpdateBookInDatabase(book, cancellationToken);
                }
                catch (Exception)
                {
                    HttpContext.Abort();
                }
            }

            Task UpdateBookInDatabase(OrderBookResponse book, CancellationToken cancellationToken = default)
            {
                var input = new CreateOrderBookInput(Core.Data.Models.OrderBook.CreateFromMarketData(book));

                // persist as fire and forget
                _ = _mediator.Using<BooksUseCases>().HandleAsync(input, cancellationToken);

                return Task.CompletedTask;
            }

            // wait until request be aborted
            HttpContext.RequestAborted.WaitHandle.WaitOne();

            return new EmptyResult();
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpGet("{currency}")]
    public async Task<IResult> CalculateAsync([FromRoute] Currency currency,
                                              [FromQuery] OrderSide side,
                                              [FromQuery] double quantity,
                                              CancellationToken cancellationToken = default)
    {
        var input = new CalculateBestPriceInput(currency, side, quantity);
        var result = await _mediator.Using<BooksUseCases>().HandleAsync(input, cancellationToken);

        return result.Match<IResult>(
            succ => Results.Ok(succ),
            fail => Results.BadRequest(fail));
    }
}
