using Aurila.Contracts.Modifiers;
using System.Globalization;

namespace Aurila.Modifiers;

internal class WeightModifier(double weight) : IAttributeModifier
{
    public void BuildAttributes(ComponentBase component, IDictionary<string, object> attributes)
    {
        string percent = (weight * 100).ToString("0.##", CultureInfo.InvariantCulture);
        attributes["style"] = $"flex: {percent}%";
    }
}
