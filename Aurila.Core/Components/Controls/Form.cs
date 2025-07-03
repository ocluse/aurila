using Aurila.Contracts.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Ocluse.LiquidSnow.Validations;

namespace Aurila.Components.Controls;

public class Form : ComponentBase, IForm
{
    private readonly HashSet<IFormControl> _inputs = [];
    private readonly HashSet<IValidatable> _valItems = [];

    [Parameter]
    [EditorRequired]
    public required RenderFragment<FormContext> ChildContent { get; set; }

    [Parameter]
    public Func<Task>? OnStart { get; set; }

    [Parameter]
    public EventCallback Submitted { get; set; }

    [Parameter]
    public Func<Exception, bool>? SubmissionError { get; set; }

    [CascadingParameter]
    public IExecutor? Executor { get; set; }

    public bool Enabled { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await StartAsync();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<FormContext>>(0);
        {
            builder.AddAttribute(1, nameof(CascadingValue<FormContext>.Value), this);
            builder.AddAttribute(2, nameof(CascadingValue<FormContext>.IsFixed), true);
            builder.AddAttribute(3, nameof(CascadingValue<FormContext>.ChildContent), ChildContent(GetContext()));
        }
        builder.CloseComponent();
    }

    public async Task StartAsync()
    {
        Enabled = false;
        if (OnStart != null)
        {
            await OnStart.Invoke();
        }
        Enabled = true;
    }

    public FormContext GetContext()
    {
        return new FormContext(Enabled, ExecuteSubmit);
    }

    void IForm.Register(IFormControl input)
    {
        _inputs.Add(input);

        if (input is IValidatable val)
        {
            _valItems.Add(val);
        }
    }

    void IForm.Unregister(IFormControl input)
    {
        _inputs.Remove(input);

        if (input is IValidatable val)
        {
            _valItems.Remove(val);
        }
    }

    private async Task ExecuteSubmit()
    {
        if (Executor is null)
        {
            await ExecuteSubmitCore();
        }
        else
        {
            await Executor.Execute(ExecuteSubmitCore);
        }
    }

    private async Task ExecuteSubmitCore()
    {
        Dictionary<IFormControl, bool> originalState = [];
        foreach (var input in _inputs)
        {
            originalState.Add(input, input.Disabled);
            input.Disabled = true;
        }

        Enabled = false;

        try
        {
            List<ValidationResult> validationResults = [];
            
            foreach(var validatable in _valItems)
            {
                var validationResult = await validatable.InvokeValidate();
            }

            bool valid = validationResults.All(r => r.IsValid);

            if (valid)
            {
                try
                {
                    await Submitted.InvokeAsync();
                }
                catch (Exception ex)
                {
                    bool handled = SubmissionError?.Invoke(ex) ?? false;

                    if (!handled)
                    {
                        throw;
                    }
                }
            }
            else
            {
                Executor?.ValidationFailed(validationResults);
            }
        }
        finally
        {
            foreach (var input in _inputs)
            {
                input.Disabled = originalState[input];
            }
            Enabled = true;
        }
    }
}
