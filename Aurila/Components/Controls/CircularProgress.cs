namespace Aurila.Components.Controls;

public class CircularProgress : ControlBase<CircularProgress>
{
    private const double SvgCanvas = 48;
    private const double DefaultStrokeWidth = 4;
    private const double DefaultGapSize = 4;
    private const double Center = 24; // SvgCanvas / 2
    private const string ViewBox = "0 0 48 48";

    [Parameter]
    public double? Progress { get; set; }

    [Parameter]
    public CssLength Size { get; set; } = new CssLength(48, CssUnit.Pixels);

    [Parameter]
    public double StrokeWidth { get; set; } = DefaultStrokeWidth;

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? TrackColor { get; set; }

    [Parameter]
    public double GapSize { get; set; } = DefaultGapSize;

    [Parameter]
    public bool RoundStrokeCap { get; set; } = true;

    private double Radius => (SvgCanvas - StrokeWidth) / 2.0;
    private double Circumference => 2.0 * Math.PI * Radius;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        string arcColor = Color ?? "currentColor";

        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, GetAppliedAttributes());
        {
            builder.OpenElement(2, "svg");
            builder.AddAttribute(3, "width", "100%");
            builder.AddAttribute(4, "height", "100%");
            builder.AddAttribute(5, "viewBox", ViewBox);
            builder.AddAttribute(6, "fill", "none");
            builder.AddAttribute(7, "aria-hidden", "true");
            {
                if (Progress.HasValue)
                {
                    RenderDeterminate(builder, arcColor);
                }
                else
                {
                    RenderIndeterminate(builder, arcColor);
                }
            }
            builder.CloseElement(); // svg
        }
        builder.CloseElement(); // div
    }

    private void RenderIndeterminate(RenderTreeBuilder b, string arcColor)
    {
        var c = Center;

        b.OpenElement(10, "circle");
        b.AddAttribute(11, "cx", c);
        b.AddAttribute(12, "cy", c);
        b.AddAttribute(13, "r", Radius);
        b.AddAttribute(14, "stroke", arcColor);
        b.AddAttribute(15, "stroke-width", StrokeWidth);
        //b.AddAttribute(16, "stroke-linecap", RoundStrokeCap ? "round" : "butt");
        b.AddAttribute(18, "style", $"--au-circular-progress-radius: {Radius}px;");
        b.CloseElement();
    }

    private void RenderDeterminate(RenderTreeBuilder b, string arcColor)
    {
        var progress = Math.Clamp(Progress!.Value, 0.0, 1.0);
        var trackColor = TrackColor ?? "transparent";
        var linecap = RoundStrokeCap ? "round" : "butt";
        var arcLength = Circumference * progress;
        var halfGap = GapSize / 2.0;
        var hasGap = GapSize > 0 && progress > 0.0 && progress < 1.0;
        var c = Center;

        // Track
        b.OpenElement(20, "circle");
        b.AddAttribute(21, "cx", c);
        b.AddAttribute(22, "cy", c);
        b.AddAttribute(23, "r", Radius);
        b.AddAttribute(24, "stroke", trackColor);
        b.AddAttribute(25, "stroke-width", StrokeWidth);
        b.AddAttribute(26, "stroke-linecap", linecap);
        if (hasGap)
        {
            var trackLen = Circumference - arcLength - (4 * halfGap);
            var trackStart = arcLength + (2 * halfGap);
            b.AddAttribute(27, "stroke-dasharray", $"{Math.Max(trackLen, 0):F3} {Circumference:F3}");
            b.AddAttribute(28, "stroke-dashoffset", $"{-trackStart:F3}");
            b.AddAttribute(29, "transform", $"rotate(-90 {c} {c})");
        }
        b.CloseElement();

        // Arc
        b.OpenElement(30, "circle");
        b.AddAttribute(31, "cx", c);
        b.AddAttribute(32, "cy", c);
        b.AddAttribute(33, "r", Radius);
        b.AddAttribute(34, "stroke", arcColor);
        b.AddAttribute(35, "stroke-width", StrokeWidth);
        b.AddAttribute(36, "stroke-linecap", linecap);
        b.AddAttribute(37, "stroke-dasharray", $"{arcLength:F3} {Circumference:F3}");
        b.AddAttribute(38, "stroke-dashoffset", "0");
        b.AddAttribute(39, "transform", $"rotate(-90 {c} {c})");
        b.CloseElement();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-circular-progress");
        builder.Add(Progress.HasValue
            ? "au-circular-progress--determinate"
            : "au-circular-progress--indeterminate");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        builder.Add("width", Size.ToString());
        builder.Add("height", Size.ToString());
    }
}