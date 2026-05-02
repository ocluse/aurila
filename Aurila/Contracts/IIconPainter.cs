using Aurila.Components.Controls;

namespace Aurila.Contracts;
public interface IIconPainter
{
     void BuildAttributes(AuIcon icon, IDictionary<string, object> attributes);
}
