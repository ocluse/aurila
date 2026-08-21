using Aurila.Design;
using Microsoft.JSInterop;
using Aurila.Enums.Input;


namespace Aurila.Components.Input;
public class AuTextField : AuFieldBase<AuTextField, string>, IHasMargin, IAsyncDisposable
{
    private string? _value;

    private ElementReference _textAreaElement;
    protected override ElementReference? FocusElement => _textAreaElement;

    private IJSObjectReference? _jsInstance;
    private DotNetObjectReference<AuTextField>? _dotNetRef;
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

    /// <summary>
    /// Determines how Enter and modifier+Enter behave. The default preserves normal textarea line
    /// breaks.
    /// </summary>
    [Parameter]
    public TextEnterBehavior EnterBehavior { get; set; } = TextEnterBehavior.NewLine;

    /// <summary>
    /// The modifier used by <see cref="EnterBehavior"/>. Shift matches the common chat-composer
    /// convention.
    /// </summary>
    [Parameter]
    public KeyboardModifier EnterModifier { get; set; } = KeyboardModifier.Shift;

    /// <summary>
    /// Determines how virtual keyboard line-break input behaves when no distinct keyboard shortcut
    /// is available.
    /// </summary>
    [Parameter]
    public VirtualEnterBehavior VirtualEnterBehavior { get; set; } = VirtualEnterBehavior.FollowUnmodifiedEnter;

    /// <summary>
    /// Invoked with the textarea's current DOM value when the configured Enter gesture submits.
    /// </summary>
    [Parameter]
    public EventCallback<string?> Submitted { get; set; }

    /// <summary>
    /// Attributes applied to the textarea itself. These override library-generated input attributes.
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }

    /// <summary>
    /// Makes final changes to the textarea's attributes after defaults and
    /// <see cref="InputAttributes"/> have been applied.
    /// </summary>
    [Parameter]
    public Action<IDictionary<string, object>>? InputAttributesBuilder { get; set; }

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
            builder.AddMultipleAttributes(1, GetInputAttributes());
            builder.AddAttribute(2, "rows", minLines);
            if(Placeholder.IsNotEmpty())
            {
                builder.AddAttribute(3, "placeholder", Placeholder);
            }
            if (Disabled)
            {
                builder.AddAttribute(4, "disabled");
            }
            if (ReadOnly)
            {
                builder.AddAttribute(5, "readonly");
            }
            builder.AddElementReferenceCapture(6, reference => _textAreaElement = reference);
        }
        builder.CloseElement(); //textarea
    }

    private Dictionary<string, object> GetInputAttributes()
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (Submitted.HasDelegate && VirtualEnterSubmits())
        {
            attributes["enterkeyhint"] = "send";
        }

        if (Submitted.HasDelegate)
        {
            string? shortcut = GetSubmitShortcut();
            if (shortcut is not null)
            {
                attributes["aria-keyshortcuts"] = shortcut;
            }
        }

        if (InputAttributes is not null)
        {
            foreach (var attribute in InputAttributes)
            {
                attributes[attribute.Key] = attribute.Value;
            }
        }

        InputAttributesBuilder?.Invoke(attributes);
        return attributes;
    }

    private bool VirtualEnterSubmits()
        => VirtualEnterBehavior switch
        {
            VirtualEnterBehavior.Submit => true,
            VirtualEnterBehavior.NewLine => false,
            _ => EnterBehavior == TextEnterBehavior.SubmitUnlessModified
        };

    private string? GetSubmitShortcut()
        => EnterBehavior switch
        {
            TextEnterBehavior.SubmitUnlessModified => "Enter",
            TextEnterBehavior.SubmitWhenModified => EnterModifier switch
            {
                KeyboardModifier.Shift => "Shift+Enter",
                KeyboardModifier.Control => "Control+Enter",
                KeyboardModifier.Alt => "Alt+Enter",
                KeyboardModifier.Meta => "Meta+Enter",
                KeyboardModifier.ControlOrMeta => "Control+Enter Meta+Enter",
                _ => null
            },
            _ => null
        };

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

    [JSInvokable]
    public async Task HandleSubmitFromJS(string value)
    {
        if (Disabled || ReadOnly || !Submitted.HasDelegate)
        {
            return;
        }

        if (_value != value)
        {
            _value = value;
            await NotifyValueChange(_value);
        }

        await Submitted.InvokeAsync(value);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            (int minLines, int maxLines) = GetLineBounds();

            _dotNetRef = DotNetObjectReference.Create(this);
            _jsInstance = await JSInterop.CreateObjectAsync(
                "TextField",
                _textAreaElement,
                maxLines,
                minLines,
                _dotNetRef,
                _value ?? string.Empty,
                EnterBehavior.ToString(),
                EnterModifier.ToString(),
                VirtualEnterBehavior.ToString(),
                Submitted.HasDelegate);
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
        bool enterBehaviorChanged = parameters.TryGetValue(nameof(EnterBehavior), out TextEnterBehavior enterBehavior)
            && enterBehavior != EnterBehavior;
        bool enterModifierChanged = parameters.TryGetValue(nameof(EnterModifier), out KeyboardModifier enterModifier)
            && enterModifier != EnterModifier;
        bool virtualBehaviorChanged = parameters.TryGetValue(nameof(VirtualEnterBehavior), out VirtualEnterBehavior virtualBehavior)
            && virtualBehavior != VirtualEnterBehavior;
        bool submittedChanged = parameters.TryGetValue(nameof(Submitted), out EventCallback<string?> submitted)
            && !submitted.Equals(Submitted);

        await base.SetParametersAsync(parameters);

        if (maxLinesChanged || minLinesChanged)
        {
            if (_jsInstance is not null)
            {
                (int nextMinLines, int nextMaxLines) = GetLineBounds();
                await _jsInstance.InvokeVoidAsync("setLineBounds", nextMinLines, nextMaxLines);
            }
        }

        if (_jsInstance is not null
            && (enterBehaviorChanged || enterModifierChanged || virtualBehaviorChanged || submittedChanged))
        {
            await _jsInstance.InvokeVoidAsync(
                "setEnterOptions",
                EnterBehavior.ToString(),
                EnterModifier.ToString(),
                VirtualEnterBehavior.ToString(),
                Submitted.HasDelegate);
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
        Dispose();
    }
}
