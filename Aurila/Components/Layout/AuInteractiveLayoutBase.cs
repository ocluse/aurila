using Aurila.Contracts.Layout;

namespace Aurila.Components.Layout;

/// <summary>
/// An optionally interactive flow-content container that also establishes itself as the layout
/// parent for its children.
/// </summary>
public abstract class AuInteractiveLayoutBase<TControl> : AuInteractiveContainerBase<TControl>, ILayoutParent
    where TControl : AuInteractiveLayoutBase<TControl>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildContainerContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void RenderContentScope(RenderTreeBuilder builder, RenderFragment content)
    {
        builder.OpenComponent<CascadingValue<ILayoutParent>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
        builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), content);
        builder.CloseComponent();
    }
}
