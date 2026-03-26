using Aurila.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Shapes;

public sealed class PillShape : IShape
{
    public void BuildClass(ComponentBase component, ClassBuilder builder) { }
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
        => builder.Add("border-radius", "9999px");
}
