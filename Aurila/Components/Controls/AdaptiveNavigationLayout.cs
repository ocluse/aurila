using Microsoft.JSInterop;

namespace Aurila.Components.Controls;

public sealed class AdaptiveNavigationLayout : ControlBase<AdaptiveNavigationLayout>, IAsyncDisposable
{
    [Parameter]
    public RenderFragment? Navigation { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IReadOnlyList<AdaptiveNavBreakpoint>? Breakpoints { get; set; }

    [Parameter]
    public AdaptiveNavPresentation? Presentation { get; set; }

    [Inject]
    public AurilaJSInterop JSInterop { get; set; } = null!;

    private DotNetObjectReference<AdaptiveNavigationLayout>? _dotNetObjRef;
    private IJSObjectReference? _jsObjectRef;
    private ElementReference _layoutElement;
    private AdaptiveNavPresentation _currentPresentation = AdaptiveNavPresentation.ExpandedRail;

    private IReadOnlyList<AdaptiveNavBreakpoint> EffectiveBreakpoints => Breakpoints ?? DefaultBreakpoints;

    private static IReadOnlyList<AdaptiveNavBreakpoint> DefaultBreakpoints { get; } =
    [
        new AdaptiveNavBreakpoint(1200, null, AdaptiveNavPresentation.ExpandedRail),
        new AdaptiveNavBreakpoint(900, 1199, AdaptiveNavPresentation.CompactRail),
        new AdaptiveNavBreakpoint(0, 899, AdaptiveNavPresentation.BottomBar)
    ];

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            builder.AddElementReferenceCapture(2, element => _layoutElement = element);

            if (IsRailPresentation(_currentPresentation))
            {
                builder.OpenElement(3, "aside");
                {
                    builder.AddAttribute(4, "class", "au-adaptive-navigation-layout__navigation");
                    builder.AddContent(5, Navigation);
                }
                builder.CloseElement();

                builder.OpenElement(6, "main");
                {
                    builder.AddAttribute(7, "class", "au-adaptive-navigation-layout__content");
                    builder.AddContent(8, ChildContent);
                }
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(9, "main");
                {
                    builder.AddAttribute(10, "class", "au-adaptive-navigation-layout__content");
                    builder.AddContent(11, ChildContent);
                }
                builder.CloseElement();

                builder.OpenElement(12, "div");
                {
                    builder.AddAttribute(13, "class", "au-adaptive-navigation-layout__navigation");
                    builder.AddContent(14, Navigation);
                }
                builder.CloseElement();
            }
        }
        builder.CloseElement();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-adaptive-navigation-layout")
            .Add($"au-adaptive-navigation-layout--{GetPresentationClass(_currentPresentation)}");
    }

    private static bool IsRailPresentation(AdaptiveNavPresentation presentation)
    {
        return presentation is AdaptiveNavPresentation.ExpandedRail or AdaptiveNavPresentation.CompactRail;
    }

    private static string GetPresentationClass(AdaptiveNavPresentation presentation)
    {
        return presentation switch
        {
            AdaptiveNavPresentation.ExpandedRail => "expanded-rail",
            AdaptiveNavPresentation.CompactRail => "compact-rail",
            AdaptiveNavPresentation.BottomBar => "bottom-bar",
            AdaptiveNavPresentation.Drawer => "drawer",
            _ => "expanded-rail"
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Presentation.HasValue)
        {
            var nextPresentation = Presentation.Value;
            if (nextPresentation != _currentPresentation)
            {
                _currentPresentation = nextPresentation;
                await InvokeAsync(StateHasChanged);
            }
            return;
        }

        if (firstRender)
        {
            _dotNetObjRef = DotNetObjectReference.Create(this);
            _jsObjectRef = await JSInterop.CreateObjectAsync(
                "AdaptiveNavigationLayoutObserver",
                _layoutElement,
                _dotNetObjRef);
        }
        else if (_jsObjectRef != null)
        {
            await _jsObjectRef.InvokeVoidAsync("setElement", _layoutElement);
        }
    }

    [JSInvokable]
    public async Task HandleLayoutWidthChanged(int width)
    {
        if (Presentation.HasValue)
        {
            return;
        }

        var matched = EffectiveBreakpoints.FirstOrDefault(item => item.Matches(width));
        var next = matched?.Presentation ?? AdaptiveNavPresentation.ExpandedRail;

        if (next != _currentPresentation)
        {
            _currentPresentation = next;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsObjectRef != null)
        {
            await _jsObjectRef.InvokeVoidAsync("dispose");
            await _jsObjectRef.DisposeAsync();
        }

        _dotNetObjRef?.Dispose();
    }
}
