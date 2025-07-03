using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Controls;

public abstract class ClickableBase<TControl> : FormControlBase<TControl>
    where TControl : ClickableBase<TControl>
{
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            if (StopPropagation)
            {
                builder.AddEventStopPropagationAttribute(3, "onclick", true);
            }
            builder.OpenRegion(4);
            {
                BuildContent(builder);
            }
            builder.CloseRegion();
        }
        builder.CloseElement();
    }

    protected abstract void BuildContent(RenderTreeBuilder builder);
}
