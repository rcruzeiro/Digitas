using Microsoft.Extensions.DependencyInjection;

namespace Digitas.Core.Shared.Mediator;

public sealed class UseCaseMediator : IUseCaseMediator
{
    private readonly IServiceProvider _serviceProvider;

    public UseCaseMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T Using<T>() where T : IUseCase
    {
        try
        {
            var useCase =
                _serviceProvider.CreateScope().ServiceProvider.GetService<T>()
                ?? throw new InvalidOperationException($"Use Case \"{typeof(T).Name}\" not implemented.");

            return useCase;
        }
        catch (Exception ex)
        {
            //TODO: error treatment
            throw;
        }
    }
}
