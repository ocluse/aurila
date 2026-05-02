using Aurila.Design;

namespace Aurila.Contracts;

public interface IStyler
{
    void BuildClass(ComponentBase component, ClassBuilder builder);
    void BuildStyle(ComponentBase component, StyleBuilder builder);
}


public interface ITextStyle : IStyler
{
}


public class AggregatingTextStyle(IEnumerable<ITextStyle> textStyles) : ITextStyle
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        foreach (var textStyle in textStyles)
        {
            textStyle.BuildClass(component, builder);
        }
    }
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        foreach (var textStyle in textStyles)
        {
            textStyle.BuildStyle(component, builder);
        }
    }
}

public class CssTextStyle(string cssClass) : ITextStyle
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        builder.Add(cssClass);
    }
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        // No styles to build for a CSS class-based text style
    }
}