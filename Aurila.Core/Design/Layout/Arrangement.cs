using Aurila.Contracts.Design;

namespace Aurila.Design.Layout;

public static class Arrangement
{
    private static SimpleLayoutArrangement Simple(string name)=> new(name);

    public static class Vertical
    {
        public static IArrangement Top { get; } = Simple("au-arrangement-vertical-top");
        public static IArrangement Center { get; } = Simple("au-arrangement-vertical-center");
        public static IArrangement Bottom { get; } = Simple("au-arrangement-vertical-bottom");

        public static IArrangement SpaceBetween { get; } = Simple("au-arrangement-vertical-space-between");
        public static IArrangement SpaceAround { get; } = Simple("au-arrangement-vertical-space-around");
        public static IArrangement SpaceEvenly { get; } = Simple("au-arrangement-vertical-space-evenly");

        public static IArrangement EqualWeight { get; } = Simple("au-arrangement-vertical-equal-weight");

        public static IArrangement SpacedBy(double px)
            => new SpacedByLayoutArrangement("au-arrangement-vertical-spaced-by", px);
    }

    public static class Horizontal
    {
        public static IArrangement Start { get; } = Simple("au-arrangement-horizontal-start");
        public static IArrangement Center { get; } = Simple("au-arrangement-horizontal-center");
        public static IArrangement End { get; } = Simple("au-arrangement-horizontal-end");

        public static IArrangement SpaceBetween { get; } = Simple("au-arrangement-horizontal-space-between");
        public static IArrangement SpaceAround { get; } = Simple("au-arrangement-horizontal-space-around");
        public static IArrangement SpaceEvenly { get; } = Simple("au-arrangement-horizontal-space-evenly");

        public static IArrangement EqualWeight { get; } = Simple("au-arrangement-horizontal-equal-weight");

        public static IArrangement SpacedBy(double px)
            => new SpacedByLayoutArrangement("au-arrangement-horizontal-spaced-by", px);
    }
}