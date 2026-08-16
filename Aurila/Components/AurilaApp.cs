using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;
using Aurila.Services.Navigation;

namespace Aurila.Components;

/// <summary>
/// The root of an Aurila application.
/// </summary>
/// <remarks>
/// Brings up the navigation ledger before any navigation host runs, so that the browser's entry list
/// is readable by the time a host asks what page to show.
/// </remarks>
public sealed class AurilaApp : ComponentBase, IAsyncDisposable
{
    private bool _isInitialized;

    [Inject]
    private JsNavigationLedger Ledger { get; set; } = null!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public NavSnapshot Snapshot => Ledger.Snapshot;

    protected override async Task OnInitializedAsync()
    {
        await Ledger.InitializeAsync();

        _isInitialized = true;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!_isInitialized)
        {
            return;
        }

        builder.OpenComponent<CascadingValue<AurilaApp>>(0);
        {
            builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
            builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
            builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), ChildContent);
        }
        builder.CloseComponent();
    }

    public async ValueTask DisposeAsync()
    {
        await Ledger.DisposeAsync();
    }
}
