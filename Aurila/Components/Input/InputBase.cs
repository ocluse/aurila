using Aurila.Contracts.Input;
using Aurila.Design;
using Microsoft.JSInterop;
using Ocluse.LiquidSnow.Data;
using Ocluse.LiquidSnow.Validations;

namespace Aurila.Components.Input;

public abstract class InputBase<TControl, TValue> : ControlBase<TControl>, IValidatable, IFormControl, IDisposable, IInputComponent, IFocusable
    where TControl : InputBase<TControl, TValue>
{
    private bool _valueHasChanged;
    private bool _disposed;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    protected virtual ElementReference? FocusElement { get; }

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
    public RenderFragment<ValidationResult?>? ValidationLabel { get; set; }

    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    [Parameter]
    public ValidationResult? Validation { get; set; }

    [Parameter]
    public EventCallback<ValidationResult?> ValidationChanged { get; set; }

    [Parameter]
    public Func<TValue?, Task<ValidationResult>>? Validate { get; set; }

    [Parameter]
    public DataFlow<TValue?>? ValueFlow { get; set; }

    [Parameter]
    public DataFlow<TValue?>? UnmanagedValueFlow { get; set; }

    [Parameter]
    public DataFlow<TValue?>? ValidatedValueFlow { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [CascadingParameter]
    private IForm? Form { get; set; }

    protected override sealed void OnInitialized()
    {
        base.OnInitialized();
        Form?.Register(this);
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var newValue = parameters.GetValueOrDefault<TValue>(nameof(Value));

        bool valueChanged = !EqualityComparer<TValue>.Default.Equals(Value, newValue);

        await base.SetParametersAsync(parameters);

        if (valueChanged && _valueHasChanged)
        {
            _valueHasChanged = false;
            ValueFlow?.Emit(Value);
            await InvokeAsync(InvokeValidate);
        }
    }

    protected async Task NotifyValueChange(TValue? newValue)
    {
        await ValueChanged.InvokeAsync(newValue);
        _valueHasChanged = true;
        UnmanagedValueFlow?.Emit(newValue);
    }

    public virtual async Task<bool> InvokeValidate()
    {
        bool result;
        if (Validate != null)
        {
            var validationResult = await Validate.Invoke(Value);
            await ValidationChanged.InvokeAsync(validationResult);
            result = validationResult.IsValid;

            if (result)
            {
                ValidatedValueFlow?.Emit(Value);
            }
        }
        else
        {
            result = true;
        }
        await InvokeAsync(StateHasChanged);

        return result;
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);

        bool hasValue = Value is string stringVal ? stringVal.IsNotEmpty() : Value != null;

        builder.Add("au-input")
            .AddIf(Validation?.IsValid == false, "au-input__validation-invalid")
            .AddIf(Validation?.IsValid == true, "au-input__validation-valid")
            .AddIf(Disabled, "au-input--disabled")
            .AddIf(ReadOnly, "au-input--readonly")
            .AddIf(hasValue, "au-input--has-value");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Form?.Unregister(this);
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
