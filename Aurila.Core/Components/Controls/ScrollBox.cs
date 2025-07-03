using Aurila.Contracts.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;
public sealed class ScrollBox : ControlBase<ScrollBox>, IScrollController, IAsyncDisposable
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public ScrollOrientation Orientation { get; set; } = ScrollOrientation.Vertical;

    [Inject]
    public AurilaJSInterop JSInterop { get; set; } = null!;

    public ElementReference ScrollElement => _element;

    private ElementReference _element;

    public event EventHandler<ScrollChangedEventArgs>? ScrollChanged;
    private DotNetObjectReference<ScrollBox>? _dotNetObjRef;
    private IJSObjectReference? _jsObjectRef;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<IScrollController>>(0);
        {
            builder.AddAttribute(1, nameof(CascadingValue<ScrollBox>.Value), this);
            builder.AddAttribute(2, nameof(CascadingValue<ScrollBox>.IsFixed), true);
            builder.AddAttribute(3, nameof(CascadingValue<ScrollBox>.ChildContent), (RenderFragment)(builder2 =>
            {
                builder2.OpenElement(4, "div");
                {
                    builder2.AddMultipleAttributes(5, GetAppliedAttributes());
                    builder2.AddElementReferenceCapture(6, element => _element = element);
                    builder2.AddContent(7, ChildContent);
                }
                builder2.CloseElement();
            }));
        }
        builder.CloseComponent();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        builder.Add("au-scroll-box")
            .AddIfElse(Orientation == ScrollOrientation.Vertical, "orientation-vertical", "orientation-horizontal");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            int throttleMilliseconds = 300;
            _dotNetObjRef = DotNetObjectReference.Create(this);
            _jsObjectRef = await JSInterop.CreateObjectAsync("ScrollBox", _element, _dotNetObjRef, throttleMilliseconds, Orientation.ToString());
        }
        else
        {
            if (_jsObjectRef != null)
            {
                await _jsObjectRef.InvokeVoidAsync("setElement", _element);
            }
        }
    }

    [JSInvokable]
    public void HandleScrollFromJS(ElementScrollValues scrollValues)
    {
        var (scrollPosition, scrollSize, clientSize) = Orientation switch
        {
            ScrollOrientation.Vertical => (scrollValues.ScrollTop, scrollValues.ScrollHeight, scrollValues.ClientHeight),
            ScrollOrientation.Horizontal => (scrollValues.ScrollLeft, scrollValues.ScrollWidth, scrollValues.ClientWidth),
            _ => throw new NotSupportedException($"Orientation {Orientation} is not supported.")
        };

        var progress = CalculateProgress(scrollPosition, scrollSize, clientSize);

        var args = new ScrollChangedEventArgs
        {
            ScrollPosition = scrollPosition,
            ScrollSize = scrollSize,
            ClientSize = clientSize,
            Progress = progress
        };

        ScrollChanged?.Invoke(this, args);
    }

    public async Task ScrollToPositionAsync(int positionPx)
    {
        await JSInterop.ScrollToPositionAsync(_element, positionPx, Orientation == ScrollOrientation.Vertical);
    }

    public async Task ScrollToPositionAsync(double progress)
    {
        progress = Math.Clamp(progress, 0, 1);

        var scrollValues = await JSInterop.GetScrollValuesAsync(_element);

        var (scrollSize, clientSize) = Orientation switch
        {
            ScrollOrientation.Vertical => (scrollValues.ScrollHeight, scrollValues.ClientHeight),
            ScrollOrientation.Horizontal => (scrollValues.ScrollWidth, scrollValues.ClientWidth),
            _ => throw new NotSupportedException($"Orientation {Orientation} is not supported.")
        };

        var maxScroll = scrollSize - clientSize;

        var position = maxScroll * progress;

        await ScrollToPositionAsync((int)position);
    }

    public async Task ScrollToEndAsync()
    {
        await JSInterop.ScrollToBottomAsync(_element);
    }

    public async Task ScrollToStartAsync()
    {
        await JSInterop.ScrollToTopAsync(_element);
    }

    private static double CalculateProgress(double position, double size, double client)
    {
        var maxScroll = size - client;
        return Math.Abs(maxScroll > 0 ? (position / maxScroll) : 0);
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