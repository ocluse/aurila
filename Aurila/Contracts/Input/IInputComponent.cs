using Ocluse.LiquidSnow.Validations;

namespace Aurila.Contracts.Input;

public interface IInputComponent : IControlComponent
{
    RenderFragment<ValidationResult?>? ValidationLabel { get; }

    ValidationResult? Validation { get; }
}
