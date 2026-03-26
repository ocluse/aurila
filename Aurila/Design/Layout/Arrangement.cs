using Aurila.Contracts.Design;
using Aurila.Models;

namespace Aurila.Design.Layout;

public static class Arrangement
{
    public static IArrangement Top { get; } = new SimpleArrangement("flex-start");
    public static IArrangement Bottom { get; } = new SimpleArrangement("flex-end");
    public static IArrangement Start { get; } = new SimpleArrangement("flex-start");
    public static IArrangement End { get; } = new SimpleArrangement("flex-end");
    public static IArrangement Center { get; } = new SimpleArrangement("center");

    public static IArrangement SpaceBetween { get; } = new SimpleArrangement("space-between");
    public static IArrangement SpaceAround { get; } = new SimpleArrangement("space-around");
    public static IArrangement SpaceEvenly { get; } = new SimpleArrangement("space-evenly");

    public static IArrangement EqualWeight { get; } = new EqualWeightArrangement();

    public static IArrangement SpacedBy(CssLength spacing) => new SpacedByArrangement(spacing);

    public static IArrangement SpacedBy(CssLength spacing, MainAxisAlignment alignment)
        => new SpacedByArrangement(spacing, alignment);
}