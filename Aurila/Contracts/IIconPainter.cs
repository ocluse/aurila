using Aurila.Components.Controls;

namespace Aurila.Contracts;
public interface IIconPainter
{
     void BuildAttributes(Icon icon, IDictionary<string, object> attributes);
}
