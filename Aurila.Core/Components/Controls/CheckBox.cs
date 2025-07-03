namespace Aurila.Components.Controls;

public class CheckBox : InputBase<CheckBox, bool>
{
    [Parameter]
    public RenderFragment<bool>? ChildContent { get; set; }

    private async Task HandleInputChange(ChangeEventArgs e)
    {
        if (!bool.TryParse(e.Value?.ToString(), out bool newValue))
        {
            newValue = false;
        }
        await NotifyValueChange(newValue);
    }

    protected override void BuildClass(ClassBuilder classBuilder)
    {
        classBuilder.Add("au-checkbox")
            .AddIfElse(Value, "au-checkbox--checked", "au-checkbox--unchecked");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());

            builder.OpenElement(3, "label");
            {
                builder.AddAttribute(4, "class", "au-checkbox__content");

                if (Id.IsNotEmpty())
                {
                    builder.AddAttribute(5, "for", Id);
                }

                if (ChildContent != null)
                {
                    builder.AddContent(6, ChildContent(Value));
                }

                builder.OpenElement(7, "input");
                {
                    builder.AddAttribute(10, "class", "au-checkbox__content__input");
                    if (Disabled)
                    {
                        builder.AddAttribute(16, "disabled");
                    }

                    if (ReadOnly)
                    {
                        builder.AddAttribute(17, "readonly");
                    }
                    builder.AddAttribute(11, "type", "checkbox");
                    builder.AddAttribute(12, "onchange", EventCallback.Factory.Create(this, HandleInputChange));
                    builder.AddAttribute(13, "checked", Value);
                }
                builder.CloseElement();

                builder.OpenElement(18, "span");
                {
                    builder.AddAttribute(19, "class", "au-checkbox_content__checkmark");
                }
                builder.CloseElement();


            }
            builder.CloseElement();

            if (Validation != null || Validation?.Message != null)
            {
                if (ValidationLabel != null)
                {
                    builder.OpenElement(20, "div");
                    {
                        builder.AddAttribute(21, "class", "validation-label");
                        builder.AddContent(22, ValidationLabel(Validation));
                    }
                    builder.CloseElement();

                }
                else
                {
                    builder.OpenElement(23, "label");
                    {
                        builder.AddAttribute(24, "class", "validation-label");
                        builder.AddAttribute(25, "role", "alert");
                        if (Validation?.Message.IsNotEmpty() == true)
                        {
                            builder.AddContent(26, Validation.Message);
                        }
                    }
                    builder.CloseElement();
                }
            }
        }
        builder.CloseElement();
    }
}
