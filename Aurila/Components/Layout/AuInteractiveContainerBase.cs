using Aurila.Components.Controls;
using Aurila.Design;
using Microsoft.JSInterop;

namespace Aurila.Components.Layout;

/// <summary>
/// Base for flow-content containers that are non-interactive by default but can optionally act as a
/// native link or an accessible action.
/// </summary>
public abstract class AuInteractiveContainerBase<TControl> : AuInteractionBase<TControl>, IAsyncDisposable
    where TControl : AuInteractiveContainerBase<TControl>
{
    private IJSObjectReference? _accessibleButton;

    [Inject]
    private AurilaJSInterop JSInterop { get; set; } = null!;

    protected virtual void BuildLeadingContent(RenderTreeBuilder builder)
    {
    }

    protected abstract void BuildContainerContent(RenderTreeBuilder builder);

    protected virtual void RenderContentScope(RenderTreeBuilder builder, RenderFragment content)
    {
        builder.AddContent(0, content);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        BuildLeadingContent(builder);

        builder.OpenRegion(100);
        RenderContentScope(builder, RenderRoot);
        builder.CloseRegion();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.AddIf(IsInteractive, "au-interactive-container");
    }

    private void RenderRoot(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, RendersAsLink ? "a" : "div");
        builder.AddMultipleAttributes(
            1,
            GetInteractionAttributes(
                RendersAsLink ? InteractionElementKind.Anchor : InteractionElementKind.Container));

        if (RendersAsLink)
        {
            AddLinkAttributes(builder, 2);
        }
        else if (IsInteractive)
        {
            builder.AddAttribute(5, "role", HasDestination ? "link" : "button");
            builder.AddAttribute(6, "tabindex", Disabled ? -1 : 0);

            if (Disabled)
            {
                builder.AddAttribute(7, "aria-disabled", "true");
            }
        }

        if (IsInteractive)
        {
            AddClickAttributes(builder, 8);
        }

        builder.OpenRegion(10);
        BuildContainerContent(builder);
        builder.CloseRegion();
        builder.AddElementReferenceCapture(11, CaptureInteractionElement);
        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        bool needsKeyboardBridge = IsInteractive && !RendersAsLink && !Disabled;

        if (_accessibleButton is null)
        {
            if (needsKeyboardBridge)
            {
                _accessibleButton = await JSInterop.CreateObjectAsync("AccessibleButton", InteractionElement);
            }

            return;
        }

        await _accessibleButton.InvokeVoidAsync("setElement", needsKeyboardBridge ? InteractionElement : null);
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessibleButton is not null)
        {
            await _accessibleButton.InvokeVoidAsync("dispose");
            await _accessibleButton.DisposeAsync();
            _accessibleButton = null;
        }
    }
}
