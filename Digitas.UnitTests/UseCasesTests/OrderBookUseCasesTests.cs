using AutoFixture;
using Digitas.Core.Data.Models;
using Digitas.Core.Data.MongoDb.Abstractions;
using Digitas.Core.Features.Books.UseCases;
using Digitas.Core.Features.Books.UseCases.Inputs;
using Digitas.Core.Shared;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using Moq;

namespace Digitas.UnitTests.UseCasesTests;

public class OrderBookUseCasesTests
{
    private readonly Fixture _fixture = new();
    private readonly List<OrderBook> _orderBook;

    // mocks
    private readonly Mock<IMongoDbRepository> _repository;
    private readonly Mock<ILogger<BooksUseCases>> _logger;
    private readonly Mock<IMongoQueryable<OrderBook>> _mongoQueryable;

    public OrderBookUseCasesTests()
    {
        _repository = new();
        _logger = new();
        _mongoQueryable = new();

        // create mocked order book
        _orderBook = new()
        {
            new OrderBook { Id = ObjectId.GenerateNewId(), Symbol = "btcusd", Asks = _fixture.Create<OrderBookData[]>(), Bids = _fixture.Create<OrderBookData[]>() },
            new OrderBook { Id = ObjectId.GenerateNewId(), Symbol = "ethusd", Asks = _fixture.Create<OrderBookData[]>(), Bids = _fixture.Create<OrderBookData[]>() }
        };

        ConfigureMocks();
    }

    [Fact]
    public async Task CalculateBestPrice_With_Invalid_Input_Should_Return_Faulted()
    {
        // Arrange
        var ct = new CancellationTokenSource();
        CalculateBestPriceInput? input = null;
        var useCase = new BooksUseCases(_logger.Object, _repository.Object);

        // Act
        var result = await useCase.HandleAsync(input!, ct.Token);

        // Assert
        Assert.True(result.IsFaulted);
    }

    [Theory]
    [InlineData(Currency.USD)]
    [InlineData(Currency.EUR)]
    [InlineData(Currency.GBP)]
    public async Task CalculateBestPrice_With_Invalid_Currency_Should_Return_Faulted(Currency currency)
    {
        // Arrange
        var ct = new CancellationTokenSource();
        var side = OrderSide.Ask;
        double quantity = 10;
        var input = new CalculateBestPriceInput(currency, side, quantity);
        var useCase = new BooksUseCases(_logger.Object, _repository.Object);

        // Act
        var result = await useCase.HandleAsync(input!, ct.Token);

        // Assert
        Assert.True(result.IsFaulted);
    }

    [Theory]
    [InlineData(Currency.BTC, OrderSide.Ask)]
    [InlineData(Currency.BTC, OrderSide.Bid)]
    [InlineData(Currency.ETH, OrderSide.Ask)]
    [InlineData(Currency.ETH, OrderSide.Bid)]
    public async Task CalculateBestPrice_Should_Succeed(Currency currency, OrderSide side)
    {
        // Arrange
        var ct = new CancellationTokenSource();
        double quantity = 10;
        var input = new CalculateBestPriceInput(currency, side, quantity);
        var useCase = new BooksUseCases(_logger.Object, _repository.Object);

        // Act
        var result = await useCase.HandleAsync(input!, ct.Token);

        // Assert
        Assert.True(result.IsSuccess);
    }

    private void ConfigureMocks()
    {
        var bookAsQueryable = _orderBook.AsQueryable();

        _mongoQueryable.As<IQueryable<OrderBook>>().Setup(_ => _.Provider).Returns(bookAsQueryable.Provider);
        _mongoQueryable.As<IQueryable<OrderBook>>().Setup(_ => _.Expression).Returns(bookAsQueryable.Expression);
        _mongoQueryable.As<IQueryable<OrderBook>>().Setup(_ => _.ElementType).Returns(bookAsQueryable.ElementType);
        _mongoQueryable.As<IQueryable<OrderBook>>().Setup(_ => _.GetEnumerator()).Returns(bookAsQueryable.GetEnumerator());

        _repository.Setup(_ => _.CreateAsync(It.IsAny<OrderBook>(), It.IsAny<CancellationToken>()))
            .Returns<OrderBook, CancellationToken>((book, ct) =>
            {
                return Task.CompletedTask;
            });

        _repository.Setup(_ => _.GetAsQueryableAsync<OrderBook>())
            .Returns(() =>
            {
                return Task.FromResult(_mongoQueryable.Object);
            });
    }
}
