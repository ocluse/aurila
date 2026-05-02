using Aurila.Contracts.Layout;
using Aurila.Design.Layout.Alignments;

namespace Aurila.Design.Layout;

public static class Alignment
{
    public static IAlignment Top { get; } = new TopAlignment();

    public static IAlignment Bottom { get; } = new BottomAlignment();

    public static IAlignment Start { get; } = new StartAlignment();

    public static IAlignment End { get; } = new EndAlignment();

    public static IAlignment Center { get; } = new CenterAlignment();

    public static IAlignment Stretch { get; } = new StretchAlignment();

    public static IAlignment TopStart { get; } = new BoxAlignment("top", "start");

    public static IAlignment TopCenter { get; } = new BoxAlignment("top", "center");

    public static IAlignment TopEnd { get; } = new BoxAlignment("top", "end");

    public static IAlignment BottomStart { get; } = new BoxAlignment("bottom", "start");

    public static IAlignment BottomCenter { get; } = new BoxAlignment("bottom", "center");

    public static IAlignment BottomEnd { get; } = new BoxAlignment("bottom", "end");
}