using Microsoft.JSInterop;

namespace Aurila.Components.Controls;
internal class PullToRefreshBox : ControlBase<PullToRefreshBox>, IAsyncDisposable
{
    [Parameter]
    public bool IsRefreshing { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? RefreshIndicatorContent { get; set; }

    [Parameter]
    public RenderFragment? PeekContent { get; set; }

    [Parameter]
    public EventCallback RefreshRequested { get; set; }

    [Inject]
    public AurilaJSInterop JSInterop { get; set; } = null!;

    private DotNetObjectReference<PullToRefreshBox>? _dotNetObjRef;
    private IJSObjectReference? _jsObjectRef;
    private ScrollBox? _scrollBox;
    bool _created;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.OpenElement(3, "div");
            {
                builder.AddAttribute(4, "class", "au-pull-to-refresh-box__peek-content");
                builder.AddContent(5, PeekContent);
            }
            builder.CloseElement();
            builder.OpenComponent<ScrollBox>(6);
            {
                builder.AddComponentReferenceCapture(7, component => _scrollBox = (ScrollBox)component);
                builder.AddComponentParameter(8, nameof(ScrollBox.Class), "au-pull-to-refresh-box__content");
                builder.AddComponentParameter(9, nameof(ScrollBox.ChildContent), (RenderFragment)BuildContent);
            }
            builder.CloseComponent();
        }
        builder.CloseElement();
    }

    private void BuildContent(RenderTreeBuilder builder)
    {
        if (IsRefreshing)
        {
            builder.OpenElement(1, "div");
            {
                builder.AddAttribute(2, "class", "au-pull-to-refresh-box__content__refresh-indicator");
                builder.AddContent(3, RefreshIndicatorContent);
            }
            builder.CloseElement();
        }
        builder.AddContent(4, ChildContent);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_created)
        {
            if (_scrollBox != null && _jsObjectRef != null && _scrollBox.ScrollElement.Context != null)
            {
                await _jsObjectRef.InvokeVoidAsync("setElement", _scrollBox.ScrollElement);
            }
        }
        else
        {
            if (_scrollBox != null && _scrollBox.ScrollElement.Context != null)
            {
                _dotNetObjRef = DotNetObjectReference.Create(this);
                _jsObjectRef = await JSInterop.CreateObjectAsync("PullToRefreshBox", _scrollBox.ScrollElement, _dotNetObjRef);
                _created = true;
            }
        }
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-pull-to-refresh-box");
    }

    [JSInvokable]
    public async Task HandleRefresh()
    {
        await InvokeAsync(RefreshRequested.InvokeAsync);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsObjectRef is not null)
        {
            await _jsObjectRef.InvokeVoidAsync("dispose");
            await _jsObjectRef.DisposeAsync();
        }

        _dotNetObjRef?.Dispose();
    }
}
