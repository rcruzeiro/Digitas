using Digitas.Core.Data;
using Digitas.Core.Data.MongoDb;
using Digitas.Core.MarketData;
using Digitas.Core.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Digitas.Core;

public static class CoreModule
{
    public static void AddCore(this IServiceCollection services, Action<MongoDbSettings> configure)
    {
        services
            .AddMarketData()
            .AddMongoDb(configure) // database repository
            .AddUseCases();
    }
}
