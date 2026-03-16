using Aurila.Components.Controls;

namespace Aurila.Appearance;
public interface IIconPainter
{
     void BuildAttributes(Icon icon, IDictionary<string, object> attributes);
}
