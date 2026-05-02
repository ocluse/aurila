namespace Aurila.Contracts.Input;
public interface IFieldComponent : IInputComponent
{
    RenderFragment? Header { get; }

    RenderFragment? Prefix { get; }

    RenderFragment? Suffix { get; }

    string? Placeholder { get; }

    void BuildInput(RenderTreeBuilder builder);
}
