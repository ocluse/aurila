using Aurila.Components.Layout.Internal;
using Aurila.Contracts.Layout;
using Aurila.Design;
using Aurila.Enums.Layout;
using Aurila.Models.Layout;
using System.Text;

namespace Aurila.Components.Layout;

public class AuGrid : AuControlBase<AuGrid>, ILayoutParent, IHasMargin, IHasPadding, IRow, IColumn
{
    private readonly string _gridId = $"au-grid-{Guid.NewGuid().ToString("N")[..8]}";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string ColumnDefinitions { get; set; } = "1fr";

    [Parameter]
    public string RowDefinitions { get; set; } = "auto";

    [Parameter]
    public IReadOnlyList<GridBreakpoint>? Breakpoints { get; set; }

    [Parameter]
    public IHorizontalAlignment? HorizontalAlignment { get; set; }

    [Parameter]
    public IVerticalAlignment? VerticalAlignment { get; set; }

    [Parameter]
    public bool Subgrid { get; set; } = false;

    [Parameter]
    public CssLength? Gap { get; set; }

    [Parameter]
    public CssLength? ColumnGap { get; set; }

    [Parameter]
    public CssLength? RowGap { get; set; }

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
        // dynamically generated CSS only if we have breakpoints:
        if (Breakpoints != null && Breakpoints.Any())
        {
            builder.OpenElement(0, "style");
            builder.AddContent(1, GenerateGridCss());
            builder.CloseElement();
        }
        builder.OpenRegion(2);
        LayoutRenderingUtility.Render(this, builder);
        builder.CloseRegion();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-grid");
        builder.Add(_gridId);

        VerticalAlignment?.BuildClass(LayoutScope.Children, this, builder);
        HorizontalAlignment?.BuildClass(LayoutScope.Children, this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        builder.Add("display", "grid");

        VerticalAlignment?.BuildStyle(LayoutScope.Children, this, builder);
        HorizontalAlignment?.BuildStyle(LayoutScope.Children, this, builder);

        if (ColumnGap != null)
        {
            builder.Add("column-gap", ColumnGap.ToString());

        }
        else if (Gap != null)
        {
            builder.Add("column-gap", Gap.ToString());
        }

        if (RowGap != null)
        {
            builder.Add("row-gap", RowGap.ToString());
        }
        else if (Gap != null)
        {
            builder.Add("row-gap", Gap.ToString());
        }

        if (Subgrid)
        {
            builder.Add("grid-column", "1 / -1");
            builder.Add("grid-template-columns", "subgrid");
            builder.Add("grid-template-rows", "auto");
        }
        else if (Breakpoints == null || !Breakpoints.Any())
        {
            builder.Add("grid-template-columns", ColumnDefinitions);
            builder.Add("grid-template-rows", RowDefinitions);
        }
    }

    private string GenerateGridCss()
    {
        var css = new StringBuilder();

        // Base (mobile-first) rules
        css.AppendLine($".{_gridId} {{");
        css.AppendLine($"  grid-template-columns: {ColumnDefinitions};");
        css.AppendLine($"  grid-template-rows: {RowDefinitions};");
        css.AppendLine("}");

        if (Breakpoints == null || !Breakpoints.Any())
            return css.ToString();

        // CSS overrides must be processed in ascending order of MinWidth 
        // so larger screens properly cascade over smaller screens.
        var sortedBreakpoints = Breakpoints.OrderBy(b => b.MinWidth);

        foreach (var bp in sortedBreakpoints)
        {
            // Build the media query string
            var mediaQuery = $"@media (min-width: {bp.MinWidth}px)";

            if (bp.MaxWidth.HasValue)
            {
                mediaQuery += $" and (max-width: {bp.MaxWidth.Value}px)";
            }

            // Append the rules for this breakpoint
            css.AppendLine($"{mediaQuery} {{");
            css.AppendLine($"  .{_gridId} {{");
            css.AppendLine($"    grid-template-columns: {bp.Definition.Columns};");
            css.AppendLine($"    grid-template-rows: {bp.Definition.Rows};");
            css.AppendLine("  }");
            css.AppendLine("}");
        }

        return css.ToString();
    }
}
