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
            if(modifier is IClassModifier classModifier)
            {
                classModifier.BuildClass(component, builder);
            }
           
        }
    }

    public void BuildAttributes(ComponentBase component, IDictionary<string, object> attributes)
    {
        foreach (var modifier in _modifiers)
        {
            if(modifier is IAttributeModifier attributeModifier)
            {
                attributeModifier.BuildAttributes(component, attributes);
            }
        }
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        foreach (var modifier in _modifiers)
        {
            if(modifier is IStyleModifier styleModifier)
            {
                styleModifier.BuildStyle(component, builder);
            }
        }
    }
}
