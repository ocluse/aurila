using Aurila.Models;
using Aurila.Design;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace Aurila.Components.Layout;

public abstract class AuHorizontalPagerBase<TControl> : AuControlBase<TControl>, IAsyncDisposable
    where TControl : AuHorizontalPagerBase<TControl>
{
    private readonly string _pagerId = $"au-pager-{Guid.NewGuid():N}";
    private DotNetObjectReference<AuHorizontalPagerBase<TControl>>? _objRef;
    private IJSObjectReference? _jsPager;
    private ElementReference _pagerElement;
    private bool _isDisposed;
    private int _lastRenderedPageCount;
    private int _lastScrollToPage = -1;

    [Inject]
    public AurilaJSInterop JSInterop { get; set; } = null!;

    [Parameter]
    public int CurrentPage { get; set; }

    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }

    [Parameter]
    public bool UserScrollEnabled { get; set; } = true;

    [Parameter]
    public CssLength? PageSpacing { get; set; }

    [Parameter]
    public PaddingValues? ContentPadding { get; set; }

    protected abstract int GetPageCount();
    
    protected abstract void BuildPageContent(int index, RenderTreeBuilder builder);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        var attrs = GetAppliedAttributes();
        if (!attrs.ContainsKey("id")) attrs["id"] = _pagerId;
        builder.AddMultipleAttributes(1, attrs);
        builder.AddElementReferenceCapture(2, element => _pagerElement = element);

        var pageCount = GetPageCount();
        for (int i = 0; i < pageCount; i++)
        {
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", "au-pager__page");
            builder.AddAttribute(5, "data-page-index", i);
            builder.AddAttribute(6, "style", "scroll-snap-align: center; flex: 0 0 100%; box-sizing: border-box;");
            BuildPageContent(i, builder);
            builder.CloseElement();
        }
        
        builder.CloseElement();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-horizontal-pager");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        builder.Add("display", "flex");
        builder.Add("overflow-x", UserScrollEnabled ? "auto" : "hidden");
        builder.Add("scroll-snap-type", "x mandatory");
        builder.Add("scrollbar-width", "none"); // Hides scrollbar on Firefox
        builder.Add("-webkit-overflow-scrolling", "touch");

        if (PageSpacing != null)
            builder.Add("column-gap", PageSpacing.ToString());

        if (ContentPadding != null)
        {
            if (ContentPadding.Top.HasValue) builder.Add("padding-top", ContentPadding.Top.ToString());
            if (ContentPadding.Bottom.HasValue) builder.Add("padding-bottom", ContentPadding.Bottom.ToString());
            if (ContentPadding.Left.HasValue) builder.Add("padding-left", ContentPadding.Left.ToString());
            if (ContentPadding.Right.HasValue) builder.Add("padding-right", ContentPadding.Right.ToString());
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var currentPageCount = GetPageCount();
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);
            _jsPager = await JSInterop.CreateObjectAsync("HorizontalPager", _pagerElement, _objRef);
            _lastRenderedPageCount = currentPageCount;

            if (CurrentPage != 0)
            {
                await _jsPager.InvokeVoidAsync("scrollToPage", CurrentPage);
                _lastScrollToPage = CurrentPage;
            }
        }
        else if (_lastRenderedPageCount != currentPageCount)
        {
            _lastRenderedPageCount = currentPageCount;
            if (_jsPager != null)
            {
                await _jsPager.InvokeVoidAsync("observeChildren");
            }
        }

        if (_jsPager != null && CurrentPage != _lastScrollToPage)
        {
            _lastScrollToPage = CurrentPage;
            await _jsPager.InvokeVoidAsync("scrollToPage", CurrentPage);
        }
    }

    [JSInvokable]
    public async Task OnPageScrolledIntoView(int newPageIndex)
    {
        if (CurrentPage != newPageIndex)
        {
            var requestedPage = newPageIndex;
            await CurrentPageChanged.InvokeAsync(newPageIndex);
            
            // Reconciliation check
            if (CurrentPage != requestedPage && _jsPager != null)
            {
                await _jsPager.InvokeVoidAsync("scrollToPage", CurrentPage);
                _lastScrollToPage = CurrentPage;
            }
            else
            {
                _lastScrollToPage = newPageIndex;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _objRef?.Dispose();
        if (_jsPager != null)
        {
            try
            {
                await _jsPager.InvokeVoidAsync("dispose");
                await _jsPager.DisposeAsync();
            }
            catch
            {
                // Ignored
            }
        }
    }
}
