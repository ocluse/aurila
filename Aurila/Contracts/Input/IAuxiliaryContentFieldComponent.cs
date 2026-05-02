namespace Aurila.Contracts.Input;

public interface IAuxiliaryContentFieldComponent : IFieldComponent
{
    void BuildAuxiliaryContent(RenderTreeBuilder builder);
}
