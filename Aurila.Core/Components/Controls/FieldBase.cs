using Aurila.Contracts.Components;
using Ocluse.LiquidSnow.Extensions;

namespace Aurila.Components.Controls;

public abstract class FieldBase<TControl, TValue> : InputBase<TControl, TValue>, IFieldComponent
    where TControl : FieldBase<TControl, TValue>
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
