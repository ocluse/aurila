using Aurila.Contracts.Layout;
using Aurila.Design.Layout.Alignments;

namespace Aurila.Design.Layout;

public static class Alignment
{
    public static IVerticalAlignment Top { get; } = new TopAlignment();

    public static IVerticalAlignment Bottom { get; } = new BottomAlignment();

    public static IHorizontalAlignment Start { get; } = new StartAlignment();

    public static IHorizontalAlignment End { get; } = new EndAlignment();

    public static IBidirectionalAlignment Center { get; } = new CenterAlignment();

    public static IBidirectionalAlignment Stretch { get; } = new StretchAlignment();

    public static IBidirectionalAlignment TopStart { get; } = new BoxAlignment("top", "start");

    public static IBidirectionalAlignment TopCenter { get; } = new BoxAlignment("top", "center");

    public static IBidirectionalAlignment TopEnd { get; } = new BoxAlignment("top", "end");

    public static IBidirectionalAlignment BottomStart { get; } = new BoxAlignment("bottom", "start");
    
    public static IBidirectionalAlignment BottomCenter { get; } = new BoxAlignment("bottom", "center");

    public static IBidirectionalAlignment BottomEnd { get; } = new BoxAlignment("bottom", "end");
}