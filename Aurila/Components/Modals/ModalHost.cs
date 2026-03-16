namespace Aurila.Components.Modals;

/// <summary>
/// Place this once at the root of your app (e.g. MainLayout.razor).
/// It renders all modal overlays registered via ModalHostService, keeping them
/// outside any ancestor stacking context or containing block that could interfere
/// with position:fixed or z-index.
/// </summary>
public class ModalHost : ComponentBase, IDisposable
{
    [Inject]
    ModalHostService HostService { get; set; } = null!;

    protected override void OnInitialized()
    {
        HostService.OnChanged += HandleChanged;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        int seq = 0;
        foreach (var registration in HostService.Registrations)
        {
            builder.AddContent(seq++, registration.Fragment);
        }
    }

    private void HandleChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        HostService.OnChanged -= HandleChanged;
    }
}
