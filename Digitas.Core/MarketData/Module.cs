using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digitas.Core.MarketData;

public static class Module
{
    public static IServiceCollection AddMarketData(this IServiceCollection services)
    {
        services.TryAddTransient<IMarketDataClient, MarketDataClient>();

        return services;
    }
}
