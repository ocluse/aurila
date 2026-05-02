using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Layout.Arrangements;

internal class SimpleArrangement(string justifyContent) : IArrangement
{
    public void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder)
    {
    }

    public void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder)
    {
        builder.Add("justify-content", justifyContent);
    }
}
