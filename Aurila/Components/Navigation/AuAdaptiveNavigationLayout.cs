using Aurila.Design;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Microsoft.JSInterop;
using System.Text;

namespace Aurila.Components.Navigation;

public sealed class AuAdaptiveNavigationLayout : AuControlBase<AuAdaptiveNavigationLayout>, IAsyncDisposable
{
    private readonly string _layoutId = $"au-nav-{Guid.NewGuid().ToString("N")[..8]}";

    [Parameter]
    public RenderFragment? Navigation { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IReadOnlyList<AdaptiveNavBreakpoint>? Breakpoints { get; set; }

    [Inject]
    public AurilaJSInterop JSInterop { get; set; } = null!;

    private DotNetObjectReference<AuAdaptiveNavigationLayout>? _dotNetObjRef;
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
        builder.OpenElement(0, "style");
        builder.AddContent(1, GenerateLayoutCss());
        builder.CloseElement();

        builder.OpenElement(2, "div");
        {
            builder.AddMultipleAttributes(3, GetAppliedAttributes());
            builder.AddElementReferenceCapture(4, element => _layoutElement = element);

            builder.OpenElement(5, "aside");
            {
                builder.AddAttribute(6, "class", "au-adaptive-navigation-layout__navigation");
                builder.AddContent(7, Navigation);
            }
            builder.CloseElement();

            builder.OpenElement(8, "main");
            {
                builder.AddAttribute(9, "class", "au-adaptive-navigation-layout__content");
                builder.AddContent(10, ChildContent);
            }
            builder.CloseElement();
        }
        builder.CloseElement();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-adaptive-navigation-layout");
        builder.Add(_layoutId);
        builder.Add($"au-adaptive-navigation-layout--{GetPresentationClass(_currentPresentation)}");
    }

    private static string GetPresentationClass(AdaptiveNavPresentation presentation)
    {
        return presentation switch
        {
            AdaptiveNavPresentation.ExpandedRail => "expanded-rail",
            AdaptiveNavPresentation.CompactRail => "compact-rail",
            AdaptiveNavPresentation.BottomBar => "bottom-bar",
            _ => "expanded-rail"
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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
        var matched = EffectiveBreakpoints.FirstOrDefault(item => item.Matches(width));
        var next = matched?.Presentation ?? AdaptiveNavPresentation.ExpandedRail;

        if (next != _currentPresentation)
        {
            _currentPresentation = next;
            await InvokeAsync(StateHasChanged);
        }
    }

    private string GenerateLayoutCss()
    {
        var css = new StringBuilder();

        var sortedBreakpoints = EffectiveBreakpoints.OrderBy(b => b.MinWidth);

        foreach (var bp in sortedBreakpoints)
        {
            var mediaQuery = $"@media (min-width: {bp.MinWidth}px)";

            if (bp.MaxWidth.HasValue)
            {
                mediaQuery += $" and (max-width: {bp.MaxWidth.Value}px)";
            }

            css.AppendLine($"{mediaQuery} {{");
            css.AppendLine($"  .{_layoutId} {{");

            if (bp.Presentation == AdaptiveNavPresentation.BottomBar)
            {
                css.AppendLine("    grid-template-areas: \"content\" \"nav\";");
                css.AppendLine("    grid-template-columns: 1fr;");
                css.AppendLine("    grid-template-rows: 1fr max-content;");
            }
            else // ExpandedRail or CompactRail
            {
                css.AppendLine("    grid-template-areas: \"nav content\";");
                css.AppendLine("    grid-template-columns: max-content 1fr;");
                css.AppendLine("    grid-template-rows: 1fr;");
            }

            css.AppendLine("  }");
            css.AppendLine("}");
        }

        return css.ToString();
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