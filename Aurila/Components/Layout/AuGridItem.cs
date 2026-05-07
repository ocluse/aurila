using Aurila.Components.Layout.Internal;
using Aurila.Contracts.Layout;
using Aurila.Design;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Layout;

public class AuGridItem : AuControlBase<AuGridItem>, ILayoutParent, IHasPadding
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool FullWidth { get; set; }

    [Parameter]
    public bool FullHeight { get; set; }

    [Parameter]
    public string? GridColumn { get; set; }

    [Parameter]
    public string? GridRow { get; set; }

    [Parameter]
    public int? Column { get; set; }

    [Parameter]
    public int? ColumnSpan { get; set; }

    [Parameter]
    public int? Row { get; set; }

    [Parameter]
    public int? RowSpan { get; set; }

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
        builder.Add("au-grid-item");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        // Columns
        if (FullWidth)
        {
            builder.Add("grid-column", "1 / -1");
        }
        else if (!string.IsNullOrWhiteSpace(GridColumn))
        {
            builder.Add("grid-column", GridColumn);
        }
        else if (Column.HasValue && ColumnSpan.HasValue)
        {
            builder.Add("grid-column", $"{Column.Value} / span {ColumnSpan.Value}");
        }
        else if (Column.HasValue)
        {
            builder.Add("grid-column-start", Column.Value.ToString());
        }
        else if (ColumnSpan.HasValue)
        {
            builder.Add("grid-column", $"span {ColumnSpan.Value}");
        }

        // Rows
        if (FullHeight)
        {
            builder.Add("grid-row", "1 / -1");
        }
        else if (!string.IsNullOrWhiteSpace(GridRow))
        {
            builder.Add("grid-row", GridRow);
        }
        else if (Row.HasValue && RowSpan.HasValue)
        {
            builder.Add("grid-row", $"{Row.Value} / span {RowSpan.Value}");
        }
        else if (Row.HasValue)
        {
            builder.Add("grid-row-start", Row.Value.ToString());
        }
        else if (RowSpan.HasValue)
        {
            builder.Add("grid-row", $"span {RowSpan.Value}");
        }
    }
}
