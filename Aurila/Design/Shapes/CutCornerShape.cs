using Aurila.Contracts.Design;

namespace Aurila.Design.Shapes;

public sealed class CutCornerShape(CssLength size) : IShape
{
    public void BuildClass(ComponentBase component, ClassBuilder builder) { }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        var s = size.ToString();
        builder.Add("clip-path",
            $"polygon({s} 0, calc(100% - {s}) 0, 100% {s}, 100% calc(100% - {s}), calc(100% - {s}) 100%, {s} 100%, 0 calc(100% - {s}), 0 {s})");
    }
}