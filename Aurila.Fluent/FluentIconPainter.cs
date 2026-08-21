using Aurila.Components.Controls;

namespace Aurila.Fluent;

/// <summary>Paints filled Fluent icons on their native 24 by 24 canvas.</summary>
public sealed class FluentIconPainter : IIconPainter
{
    public void BuildAttributes(AuIcon icon, IDictionary<string, object> attributes)
    {
        attributes["viewBox"] = "0 0 24 24";
        attributes["fill"] = icon.Color ?? "currentColor";
        attributes.Remove("stroke");
        attributes.Remove("stroke-width");
        attributes.Remove("stroke-linecap");
    }
}
