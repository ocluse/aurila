using Ocluse.LiquidSnow.Validations;

namespace Aurila.Contracts.Components;
public interface IFieldComponent : IInputComponent
{
    RenderFragment? Header { get; }

    RenderFragment? Prefix { get; }

    RenderFragment? Suffix { get; }

    string? Placeholder { get; }

    void BuildInput(RenderTreeBuilder builder);
}

public interface IControlComponent
{
    IEnumerable<KeyValuePair<string, object>> GetAppliedAttributes();
}

public interface IAuxiliaryContentFieldComponent : IFieldComponent
{
    void BuildAuxiliaryContent(RenderTreeBuilder builder);
}

public interface IInputComponent : IControlComponent
{
    RenderFragment<ValidationResult?>? ValidationLabel { get; }

    ValidationResult? Validation { get; }
}
