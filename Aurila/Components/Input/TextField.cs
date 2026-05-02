using Aurila.Design;
using Microsoft.JSInterop;


namespace Aurila.Components.Input;
public class TextField : FieldBase<TextField, string>
{
    private string? _value;

    private ElementReference _textAreaElement;
    protected override ElementReference? FocusElement => _textAreaElement;

    private IJSObjectReference? _jsInstance;
    private DotNetObjectReference<TextField>? _dotNetRef;
    private bool _isFocused;
    private bool _isComposing;
    private string? _pendingExternalValue;
    private long _pendingExternalVersion;
    private long _externalVersion;

    [Inject]
    private AurilaJSInterop JSInterop { get; set; } = null!;

    [Parameter]
    public int MaxLines { get; set; } = 4;

    [Parameter]
    public int MinLines { get; set; } = 1;

    [Parameter]
    public bool DeferExternalUpdatesWhileFocused { get; set; } = true;

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-text-field");
    }

    protected override void BuildInput(RenderTreeBuilder builder)
    {
        (int minLines, _) = GetLineBounds();

        builder.OpenElement(0, "textarea");
        {
            builder.AddAttribute(1, "rows", minLines);
            if(Placeholder.IsNotEmpty())
            {
                builder.AddAttribute(2, "placeholder", Placeholder);
            }
            if (Disabled)
            {
                builder.AddAttribute(3, "disabled");
            }
            if (ReadOnly)
            {
                builder.AddAttribute(4, "readonly");
            }
            builder.AddElementReferenceCapture(5, reference => _textAreaElement = reference);
        }
        builder.CloseElement(); //textarea
    }

    [JSInvokable]
    public async Task HandleInputFromJS(string value, int? selectionStart, int? selectionEnd, bool isComposing)
    {
        _value = value;
        _isComposing = isComposing;

        _ = selectionStart;
        _ = selectionEnd;

        await NotifyValueChange(_value);
    }

    [JSInvokable]
    public Task HandleFocusFromJS()
    {
        _isFocused = true;
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task HandleBlurFromJS()
    {
        _isFocused = false;
        _isComposing = false;
        return FlushPendingExternalValueAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            (int minLines, int maxLines) = GetLineBounds();

            _dotNetRef = DotNetObjectReference.Create(this);
            _jsInstance = await JSInterop.CreateObjectAsync("TextField", _textAreaElement, maxLines, minLines, _dotNetRef, _value ?? string.Empty);
            await FlushPendingExternalValueAsync();
        }
        else if(_jsInstance != null)
        {
            await _jsInstance.InvokeVoidAsync("adjustHeight");
            await FlushPendingExternalValueAsync();
        }
    }

    protected override void OnParametersSet()
    {
        if (_value != Value)
        {
            _value = Value;
            _pendingExternalValue = _value ?? string.Empty;
            _pendingExternalVersion = ++_externalVersion;
        }
        base.OnParametersSet();
    }

    private async Task FlushPendingExternalValueAsync()
    {
        if (_jsInstance is null || _pendingExternalValue is null)
        {
            return;
        }

        if (DeferExternalUpdatesWhileFocused && (_isFocused || _isComposing))
        {
            return;
        }

        await _jsInstance.InvokeVoidAsync("applyExternalValue", _pendingExternalValue, _pendingExternalVersion);
        _pendingExternalValue = null;
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        bool maxLinesChanged = parameters.TryGetValue(nameof(MaxLines), out int maxLines) && maxLines != MaxLines;
        bool minLinesChanged = parameters.TryGetValue(nameof(MinLines), out int minLines) && minLines != MinLines;

        await base.SetParametersAsync(parameters);

        if (maxLinesChanged || minLinesChanged)
        {
            if (_jsInstance is not null)
            {
                (int nextMinLines, int nextMaxLines) = GetLineBounds();
                await _jsInstance.InvokeVoidAsync("setLineBounds", nextMinLines, nextMaxLines);
            }
        }
    }

    private (int MinLines, int MaxLines) GetLineBounds()
    {
        int minLines = Math.Max(1, MinLines);
        int maxLines = Math.Max(minLines, MaxLines);
        return (minLines, maxLines);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsInstance != null)
        {
            await _jsInstance.InvokeVoidAsync("dispose");
            await _jsInstance.DisposeAsync();
            _jsInstance = null;
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}
