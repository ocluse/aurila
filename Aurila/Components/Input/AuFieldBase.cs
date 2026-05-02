using Aurila.Components.Input.Internal;
using Aurila.Contracts.Input;
using Aurila.Design;

namespace Aurila.Components.Input;

public abstract class AuFieldBase<TControl, TValue> : AuInputBase<TControl, TValue>, IFieldComponent
    where TControl : AuFieldBase<TControl, TValue>
{
    [Parameter]
    public RenderFragment? Header { get; set; }

    [Parameter]
    public RenderFragment? Prefix { get; set; }

    [Parameter]
    public RenderFragment? Suffix { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }
    
    protected abstract void BuildInput(RenderTreeBuilder builder);

    void IFieldComponent.BuildInput(RenderTreeBuilder builder)
    {
        BuildInput(builder);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        FieldRenderingUtility.BuildField(builder, this, AppearanceProvider);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-field")
            .AddIf(Header != null, "au-field-input--has-header")
            .AddIf(Prefix != null, "au-field--has-prefix")
            .AddIf(Suffix != null, "au-field--has-suffix")
            .AddIf(Placeholder.IsNotEmpty(), "au-field--has-placeholder");
    }
}
