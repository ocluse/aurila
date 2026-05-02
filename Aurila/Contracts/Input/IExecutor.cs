using Ocluse.LiquidSnow.Validations;

namespace Aurila.Contracts.Input;

public interface IExecutor
{
    Task Execute(Func<Task> action);

    Task<T> Execute<T>(Func<Task<T>> action);

    void ValidationFailed(IReadOnlyCollection<ValidationResult> validationResults);
}
