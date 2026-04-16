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
        var circ = Circumference;
        var c = Center;

        // Equivalent to CSS --1deg
        var d = circ / 360.0;

        // Calculate the dasharray values for the 3 keyframe states (0%, 50%, 100%)
        // Format is always: dash, gap, dash, gap
        string v0 = $"0 0 {2 * d:F3} {358 * d:F3}";
        string v50 = $"0 {35 * d:F3} {290 * d:F3} {35 * d:F3}";
        string v100 = $"0 {358 * d:F3} {2 * d:F3} 0";
        string values = $"{v0}; {v50}; {v100}";

        b.OpenElement(10, "circle");
        b.AddAttribute(11, "cx", c);
        b.AddAttribute(12, "cy", c);
        b.AddAttribute(13, "r", Radius);
        b.AddAttribute(14, "stroke", arcColor);
        b.AddAttribute(15, "stroke-width", StrokeWidth);
        //b.AddAttribute(16, "stroke-linecap", RoundStrokeCap ? "round" : "butt");
        b.AddAttribute(17, "class", "au-cpi-arc");

        // 1. Dash Array Animation (Replaces 'dash-anim' keyframes)
        b.OpenElement(18, "animate");
        b.AddAttribute(19, "attributeName", "stroke-dasharray");
        b.AddAttribute(20, "values", values);
        b.AddAttribute(21, "keyTimes", "0; 0.5; 1");
        // This cubic-bezier spline is the mathematical equivalent to CSS 'ease-in-out'
        b.AddAttribute(22, "calcMode", "spline");
        b.AddAttribute(23, "keySplines", "0.42 0 0.58 1; 0.42 0 0.58 1");
        b.AddAttribute(24, "dur", "1.4s");
        b.AddAttribute(25, "repeatCount", "indefinite");
        b.CloseElement(); // animate

        // 2. Full Rotation Animation (Replaces 'full-rotation-anim' keyframes)
        b.OpenElement(26, "animateTransform");
        b.AddAttribute(27, "attributeName", "transform");
        b.AddAttribute(28, "type", "rotate");
        // SVG rotation format is: "angle centerX centerY"
        b.AddAttribute(29, "from", $"0 {c} {c}");
        b.AddAttribute(30, "to", $"360 {c} {c}");
        b.AddAttribute(31, "dur", "2s");
        b.AddAttribute(32, "repeatCount", "indefinite");
        b.CloseElement(); // animateTransform

        b.CloseElement(); // circle
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