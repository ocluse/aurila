using Aurila.Components;
using Aurila.Contracts.Modifiers;

namespace Aurila.Modifiers;

public class ModifiersBuilder
{
    private readonly List<IModifier> _modifiers = [];

    public ModifiersBuilder Add(IModifier modifier)
    {
        _modifiers.Add(modifier);
        return this;
    }

    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        foreach (var modifier in _modifiers)
        {
            modifier.BuildClass(component, builder);
        }
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        foreach (var modifier in _modifiers)
        {
            modifier.BuildStyle(component, builder);
        }
    }
}
