namespace Aurila.Design.Shapes;

internal sealed class NoneShape : IShape
{
    public void BuildClass(ComponentBase component, ClassBuilder builder) { }

    public void BuildStyle(ComponentBase component, StyleBuilder builder) { }
}