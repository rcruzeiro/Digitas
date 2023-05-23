using LanguageExt.Common;

namespace Digitas.Core.Shared.Mediator;

public interface IUseCaseHandler<in T>
    where T : IInput
{
    Task HandleAsync(T input, CancellationToken cancellationToken = default);
}

public interface IUseCaseHandler<in T, TOutput>
    where T : IInput
{
    Task<Result<TOutput>> HandleAsync(T input, CancellationToken cancellationToken = default);
}
