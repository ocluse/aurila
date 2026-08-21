using Aurila.Contracts.Input;
using Aurila.Design;

namespace Aurila.Components.Controls;

/// <summary>
/// Base for native clickables whose content is valid inside a button or anchor.
/// </summary>
/// <remarks>
/// A clickable renders as an anchor when it has a resolvable destination and as a button otherwise.
/// A disabled clickable always renders as a button.
/// </remarks>
public abstract class AuClickableBase<TControl> : AuInteractionBase<TControl>, IFormControl, IDisposable
    where TControl : AuClickableBase<TControl>
{
    private bool _disposed;

    [CascadingParameter]
    public IForm? Form { get; set; }

    /// <summary>
    /// Whether this control submits the enclosing form.
    /// </summary>
    [Parameter]
    public bool Submit { get; set; }

    protected override bool IsInteractive => true;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Form?.Register(this);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Submit && HasDestination)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} has both Submit and a destination. A control either submits a form " +
                "or navigates; it cannot do both.");
        }
    }

    protected virtual void BuildControlClass(ClassBuilder builder)
    {
    }

    protected override sealed void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        BuildControlClass(builder);
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);

        if (!RendersAsLink)
        {
            attributes["role"] = "button";
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, RendersAsLink ? "a" : "button");
        builder.AddMultipleAttributes(
            1,
            GetInteractionAttributes(
                RendersAsLink ? InteractionElementKind.Anchor : InteractionElementKind.Button));

        if (RendersAsLink)
        {
            AddLinkAttributes(builder, 2);
        }
        else
        {
            builder.AddAttribute(5, "type", Submit ? "submit" : "button");

            if (Disabled)
            {
                builder.AddAttribute(6, "disabled", true);
            }
        }

        AddClickAttributes(builder, 7);

        builder.OpenRegion(9);
        BuildContent(builder);
        builder.CloseRegion();
        builder.AddElementReferenceCapture(10, CaptureInteractionElement);
        builder.CloseElement();
    }

    protected abstract void BuildContent(RenderTreeBuilder builder);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Form?.Unregister(this);
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
