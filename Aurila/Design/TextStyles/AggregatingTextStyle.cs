using Aurila.Contracts.Design;

namespace Aurila.Design.TextStyles;

public class AggregatingTextStyle(string? elementName, IEnumerable<IStyler> styles) : ITextStyle
{
    public string? ElementName => elementName;

    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        foreach (var styler in styles)
        {
            styler.BuildClass(component, builder);
        }
    }
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        foreach (var styler in styles)
        {
            styler.BuildStyle(component, builder);
        }
    }
}
