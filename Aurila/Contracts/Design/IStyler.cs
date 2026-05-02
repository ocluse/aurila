using Aurila.Design;

namespace Aurila.Contracts.Design;

public interface IStyler
{
    void BuildClass(ComponentBase component, ClassBuilder builder);
    void BuildStyle(ComponentBase component, StyleBuilder builder);
}
