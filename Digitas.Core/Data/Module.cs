using Digitas.Core.Data.MongoDb;
using Digitas.Core.Data.MongoDb.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digitas.Core.Data;

public static class Module
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, Action<MongoDbSettings> configure)
    {
        var settings = new MongoDbSettings();
        configure(settings);

        // we can use a "resolver" here (w/ delegate) to allow more than 1 repository to coexist
        services.TryAddSingleton<IMongoDbRepository>(provider =>
        {
            return new MongoDbRepository(settings);
        });

        return services;
    }
}
