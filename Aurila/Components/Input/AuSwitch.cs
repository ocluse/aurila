using Aurila.Design;

namespace Aurila.Components.Input;

public class AuSwitch : AuInputBase<AuSwitch, bool>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, OnClickAsync));

            builder.OpenElement(3, "label");
            {
                builder.AddAttribute(4, "class", "au-switch__content");
                builder.OpenElement(5, "span");
                {
                    builder.AddAttribute(6, "class", "au-switch__content__slider");
                }
                builder.CloseElement(); //span
            }
            builder.CloseElement(); //label

            builder.AddContent(7, ChildContent);
        }
        builder.CloseElement(); //div
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);

        builder.Add("au-switch")
            .AddIf(Value, "au-switch--checked");
    }

    private async Task OnClickAsync()
    {
        var newValue = !Value;
        await NotifyValueChange(newValue);
    }
}
