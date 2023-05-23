using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Digitas.Core.Shared.Mediator;

public static class Module
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.TryAddSingleton<IUseCaseMediator, UseCaseMediator>();

        var types = new List<Type>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            types.AddRange(
                assembly.GetTypes()
                .Where(i => i.GetInterfaces().Any(_ => _.IsGenericType &&
                (typeof(IUseCaseHandler<,>).Equals(_.GetGenericTypeDefinition())
              || typeof(IUseCaseHandler<>).Equals(_.GetGenericTypeDefinition())) && !i.IsInterface && !i.IsAbstract)).ToList());

            foreach (var type in types)
            {
                services.TryAddScoped(type);
            }
        }

        return services;
    }
}
