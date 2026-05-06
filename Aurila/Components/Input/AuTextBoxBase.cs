using Aurila.Design;
using Aurila.Enums.Input;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Input;

public abstract class AuTextBoxBase<TControl, TValue> : AuFieldBase<TControl, TValue>, IHasMargin
    where TControl : AuTextBoxBase<TControl, TValue>
{
    [Parameter]
    public UpdateTrigger UpdateTrigger { get; set; }

    [Parameter]
    public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    [Parameter]
    public EventCallback OnReturn { get; set; }

    [Parameter]
    public CssLength? Margin { get; set; }

    [Parameter]
    public CssLength? MarginHorizontal { get; set; }

    [Parameter]
    public CssLength? MarginVertical { get; set; }

    [Parameter]
    public CssLength? MarginRight { get; set; }

    [Parameter]
    public CssLength? MarginLeft { get; set; }

    [Parameter]
    public CssLength? MarginTop { get; set; }

    [Parameter]
    public CssLength? MarginBottom { get; set; }

    protected abstract string GetInputType();

    protected abstract TValue? GetValue(object? value);

    private ElementReference _inputElement;
    
    protected override ElementReference? FocusElement => _inputElement;

    protected override void BuildInput(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "input");
        {
            builder.AddAttribute(2, "placeholder", Placeholder ?? " ");
            builder.AddAttribute(3, "type", GetInputType());
            builder.AddAttribute(4, "value", GetInputDisplayValue(Value));
            builder.AddAttribute(5, "class", "input");
            var valueUpdateCallback = EventCallback.Factory.CreateBinder(this, RuntimeHelpers.CreateInferredBindSetter(callback: HandleValueUpdated, value: Value), Value);

            builder.AddAttribute(7, UpdateTrigger.ToHtmlAttribute(), valueUpdateCallback);
            builder.SetUpdatesAttributeName("value");

            builder.AddAttribute(8, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, KeyDown));

            if (Disabled)
            {
                builder.AddAttribute(9, "disabled");
            }

            if (ReadOnly)
            {
                builder.AddAttribute(10, "readonly");
            }

            builder.AddElementReferenceCapture(11, __inputReference => _inputElement = __inputReference);
        }
        builder.CloseElement();
    }

    private async Task HandleValueUpdated(TValue? value)
    {
        ChangeEventArgs e = new()
        {
            Value = value
        };

        await HandleInputChange(e);
    }

    private async Task KeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Code == "NumpadEnter")
        {
            await OnReturn.InvokeAsync();
        }
        await OnKeyDown.InvokeAsync(e);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        builder.Add("au-textbox-base");
        base.BuildClass(builder);
    }

    protected async Task HandleInputChange(ChangeEventArgs e)
    {
        var newValue = GetValue(e.Value);
        await NotifyValueChange(newValue);
    }

    protected virtual object? GetInputDisplayValue(TValue? value)
    {
        return BindConverter.FormatValue(value);
    }
}
