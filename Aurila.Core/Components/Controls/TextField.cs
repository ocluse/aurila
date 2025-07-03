using Microsoft.JSInterop;


namespace Aurila.Components.Controls;
public class TextField : FieldBase<TextField, string>
{
    private string? _value;

    private ElementReference _textAreaElement;
    private IJSObjectReference? _jsInstance;

    [Inject]
    private AurilaJSInterop JSInterop { get; set; } = null!;

    [Parameter]
    public int MaxLines { get; set; } = 4;

    protected override void BuildInput(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "textarea");
        {
            builder.AddAttribute(1, "rows", 1);
            builder.AddElementReferenceCapture(2, reference => _textAreaElement = reference);
            if(Placeholder.IsNotEmpty())
            {
                builder.AddAttribute(3, "placeholder", Placeholder);
            }
            builder.AddAttribute(4, "oninput", HandleOnChange);
            builder.AddAttribute(5, "bind", _value);
        }
        builder.CloseElement();
    }

    private async Task HandleOnChange(ChangeEventArgs args)
    {
        _value = args.Value?.ToString();

        await NotifyValueChange(_value);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            _jsInstance = await JSInterop.CreateObjectAsync("TextField", _textAreaElement, MaxLines);
        }
        else if(_jsInstance != null)
        {
            await _jsInstance.InvokeVoidAsync("adjustHeight");
        }
    }

    protected override void OnParametersSet()
    {
        if (_value != Value)
        {
            _value = Value;
        }
        base.OnParametersSet();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        bool maxLinesChanged = parameters.TryGetValue(nameof(MaxLines), out int maxLines) && maxLines != MaxLines;

        await base.SetParametersAsync(parameters);

        if (maxLinesChanged)
        {
            if (_jsInstance is not null)
            {
                await _jsInstance.InvokeVoidAsync("setMaxLines", maxLines);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsInstance != null)
        {
            await _jsInstance.InvokeVoidAsync("dispose");
            await _jsInstance.DisposeAsync();
            _jsInstance = null;
        }
    }
}
