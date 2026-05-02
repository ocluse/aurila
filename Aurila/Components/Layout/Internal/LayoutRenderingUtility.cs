using Aurila.Contracts.Layout;

namespace Aurila.Components.Layout.Internal;

internal static class LayoutRenderingUtility
{
    public static void Render(ILayoutParent layout, RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<ILayoutParent>>(0);
        {
            builder.AddAttribute(1, "Value", layout);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment)((builder2) =>
            {
                builder2.OpenElement(4, "div");
                {
                    if(layout is IControlComponent control)
                    {
                        builder2.AddMultipleAttributes(5, control.GetAppliedAttributes());
                    }
                    
                    builder2.AddContent(6, layout.ChildContent);
                }
                builder2.CloseElement();
            }));
        }
        builder.CloseComponent();
    }
}
