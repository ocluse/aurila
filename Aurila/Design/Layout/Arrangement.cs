using Aurila.Contracts.Layout;
using Aurila.Design.Layout.Arrangements;

namespace Aurila.Design.Layout;

public static class Arrangement
{
    public static IVerticalArrangement Top { get; } = new SimpleArrangement("start");
    public static IVerticalArrangement Bottom { get; } = new SimpleArrangement("end");
    public static IHorizontalArrangement Start { get; } = new SimpleArrangement("start");
    public static IHorizontalArrangement End { get; } = new SimpleArrangement("end");
    public static IBidirectionalArrangement Center { get; } = new SimpleArrangement("center");

    public static IBidirectionalArrangement SpaceBetween { get; } = new SimpleArrangement("space-between");
    public static IBidirectionalArrangement SpaceAround { get; } = new SimpleArrangement("space-around");
    public static IBidirectionalArrangement SpaceEvenly { get; } = new SimpleArrangement("space-evenly");

    public static IBidirectionalArrangement EqualWeight { get; } = new EqualWeightArrangement();
}