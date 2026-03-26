using Aurila.Components;

namespace Aurila.Contracts.Design;

public interface IShape
{
    void BuildClass(ComponentBase component, ClassBuilder builder);
    void BuildStyle(ComponentBase component, StyleBuilder builder);
}
