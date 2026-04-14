using Aurila.Contracts.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Aurila.Components.Controls;

public abstract class ClickableBase<TControl> : FormControlBase<TControl>, IFocusable
    where TControl : ClickableBase<TControl>
{
    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    public virtual async Task FocusAsync()
    {
        if (FocusElement.HasValue)
        {
            await FocusElement.Value.FocusAsync();
        }
    }

    public virtual async Task BlurAsync()
    {
        if (FocusElement.HasValue)
        {
            await JSRuntime.InvokeVoidAsync("HTMLElement.prototype.blur.call", FocusElement.Value);
        }
    }

    [Parameter]
    public EventCallback<MouseEventArgs> Clicked { get; set; }

    [Parameter]
    public bool StopPropagation { get; set; }

    protected virtual void BuildControlClass(ClassBuilder builder) { }

    protected override sealed void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        BuildControlClass(builder);

        builder.Add("au-clickable");
        if (Disabled)
        {
            builder.Add("au-clickable--disabled");
        }
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);

        if (!Disabled && Clicked.HasDelegate)
        {
            attributes["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, Clicked);
        }

        if (Disabled)
        {
            attributes["disabled"] = true;
        }

        attributes["role"] = "button";
    }

    private ElementReference _buttonElement;

    protected virtual ElementReference? FocusElement => _buttonElement;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            
            if (StopPropagation)
            {
                builder.AddEventStopPropagationAttribute(2, "onclick", true);
            }
            builder.OpenRegion(3);
            {
                BuildContent(builder);
            }
            builder.CloseRegion();

            builder.AddElementReferenceCapture(4, __buttonRef => _buttonElement = __buttonRef);
        }
        builder.CloseElement();
    }

    protected abstract void BuildContent(RenderTreeBuilder builder);
}
