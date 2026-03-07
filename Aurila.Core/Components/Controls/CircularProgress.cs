using System;
using System.Collections.Generic;
using System.Text;

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
    
    private readonly string _uid = Guid.NewGuid().ToString("N")[..8];
    
    private string MaskId => $"au-cpi-mask-{_uid}";
    private string ClipRingId => $"au-cpi-ring-{_uid}";


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

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
                    RenderDeterminate(builder);
                else
                    RenderIndeterminate(builder);
            }
            builder.CloseElement(); // svg
        }
        builder.CloseElement(); // div
    }

    private void RenderIndeterminate(RenderTreeBuilder b)
    {
        var arcColor = Color ?? "currentColor";
        var circ = Circumference;
        var c = Center;

        // stroke-dasharray and stroke-dashoffset must be CSS properties (set via
        // style=) not SVG presentation attributes so that calc() and custom
        // property references resolve correctly during animation.
        //
        // arc visible length = (head - tail) * circumference
        // arc start position = circumference/4 - tail*circumference
        //   (the /4 offset rotates the start point to 12 o'clock)
        var arcStyle = string.Join(";",
            $"stroke-dasharray: calc((var(--au-cpi-head) - var(--au-cpi-tail)) * {circ:F3}) 9999",
            $"stroke-dashoffset: calc({circ / 4.0:F3} - var(--au-cpi-tail) * {circ:F3})"
        );

        b.OpenElement(10, "circle");
        b.AddAttribute(11, "cx", c);
        b.AddAttribute(12, "cy", c);
        b.AddAttribute(13, "r", Radius);
        b.AddAttribute(14, "stroke", arcColor);
        b.AddAttribute(15, "stroke-width", StrokeWidth);
        b.AddAttribute(16, "stroke-linecap", RoundStrokeCap ? "round" : "butt");
        b.AddAttribute(17, "class", "au-cpi-arc");
        b.AddAttribute(18, "style", arcStyle);
        b.CloseElement();
    }

    private void RenderDeterminate(RenderTreeBuilder b)
    {
        var progress = Math.Clamp(Progress!.Value, 0.0, 1.0);
        var arcColor = Color ?? "currentColor";
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

        builder.Add("--au-cpi-circ", $"{Circumference:F3}");
    }
}