using Aurila.Design;
using Aurila.Enums.Layout;

namespace Aurila.Contracts.Layout;

public interface IAlignment
{
    void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder);

    void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder);
}
