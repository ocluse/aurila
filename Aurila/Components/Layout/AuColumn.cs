using Aurila.Components.Layout.Internal;
using Aurila.Contracts.Layout;
using Aurila.Design;
using Aurila.Enums.Layout;

namespace Aurila.Components.Layout;

public class AuColumn : AuControlBase<AuColumn>, ILayoutParent, IColumn, IHasMargin, IHasPadding
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Wrap { get; set; }

    [Parameter]
    public IArrangement? VerticalArrangement { get; set; }

    [Parameter]
    public IArrangement? HorizontalArrangement { get; set; }

    [Parameter]
    public IHorizontalAlignment? HorizontalAlignment { get; set; }

    [Parameter]
    public CssLength? Gap { get; set; }

    [Parameter]
    public CssLength? Margin { get; set; }

    [Parameter]
    public CssLength? MarginHorizontal { get; set; }

    [Parameter]
    public CssLength? MarginVertical { get; set; }

    [Parameter]
    public CssLength? MarginRight { get; set; }

    [Parameter]
    public CssLength? MarginLeft { get; set; }

    [Parameter]
    public CssLength? MarginTop { get; set; }

    [Parameter]
    public CssLength? MarginBottom { get; set; }

    [Parameter]
    public CssLength? Padding { get; set; }

    [Parameter]
    public CssLength? PaddingHorizontal { get; set; }

    [Parameter]
    public CssLength? PaddingVertical { get; set; }

    [Parameter]
    public CssLength? PaddingTop { get; set; }

    [Parameter]
    public CssLength? PaddingBottom { get; set; }

    [Parameter]
    public CssLength? PaddingRight { get; set; }

    [Parameter]
    public CssLength? PaddingLeft { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        LayoutRenderingUtility.Render(this, builder);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-column");
        VerticalArrangement?.BuildClass(Axis.Vertical, this, builder);
        HorizontalArrangement?.BuildClass(Axis.Horizontal, this, builder);
        HorizontalAlignment?.BuildClass(LayoutScope.Children, this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        if (Wrap)
        {
            builder.Add("flex-wrap", "wrap");
        }

        VerticalArrangement?.BuildStyle(Axis.Vertical, this, builder);
        HorizontalArrangement?.BuildStyle(Axis.Horizontal, this, builder);
        HorizontalAlignment?.BuildStyle(LayoutScope.Children, this, builder);
        if (Gap != null)
        {
            builder.Add("gap", Gap.ToString());
        }
    }
}
