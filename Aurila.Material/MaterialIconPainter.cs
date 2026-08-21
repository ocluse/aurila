using Aurila.Components.Controls;

namespace Aurila.Material;

/// <summary>
/// Adapts <c>AuIcon</c> to Material Symbols geometry: those paths are filled shapes on a
/// <c>0 -960 960 960</c> canvas, whereas Aurila defaults to a stroked <c>0 0 24 24</c> one.
/// </summary>
public sealed class MaterialIconPainter : IIconPainter
{
    public string ViewBox { get; init; } = "0 -960 960 960";

    public void BuildAttributes(AuIcon icon, IDictionary<string, object> attributes)
    {
        attributes["viewBox"] = ViewBox;
        attributes["fill"] = icon.Color ?? "currentColor";

        attributes.Remove("stroke");
        attributes.Remove("stroke-width");
        attributes.Remove("stroke-linecap");
    }
}
