using Aurila.Contracts.Input;

namespace Aurila.Components.Input;
public abstract class FormControlBase<TControl> : ControlBase<TControl>, IFormControl, IDisposable
    where TControl : FormControlBase<TControl>
{
    private bool disposedValue;

    [CascadingParameter]
    public IForm? Form { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Form?.Register(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Form?.Unregister(this);
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
