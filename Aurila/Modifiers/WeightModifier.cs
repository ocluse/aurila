using Aurila.Contracts.Modifiers;
using System.Globalization;

namespace Aurila.Modifiers;

internal class WeightModifier(double weight) : IAttributeModifier
{
    public void BuildAttributes(ComponentBase component, IDictionary<string, object> attributes)
    {
        attributes["style"] = $"flex: {weight}";
    }
}
