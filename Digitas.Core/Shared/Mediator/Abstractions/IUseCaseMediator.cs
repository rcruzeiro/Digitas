namespace Digitas.Core.Shared.Mediator;

public interface IUseCaseMediator
{
    T Using<T>() where T : IUseCase;
}
