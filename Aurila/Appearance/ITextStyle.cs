using Aurila.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Text;

namespace Aurila.Appearance;

public interface IStyler
{
    void BuildClass(ComponentBase component, ClassBuilder builder);
    void BuildStyle(ComponentBase component, StyleBuilder builder);
}


public interface ITextStyle : IStyler
{
}

public interface IColor : IStyler
{
}

public interface IForegroundColor : IColor
{
}

public interface IBackgroundColor : IColor
{
}

public class ClassNameForegroundColor(string className) : IForegroundColor
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        builder.Add(className);
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        return;
    }
}

public class StyleForegroundColor(string cssValue) : IColor
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        return;
    }
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("color", cssValue);
    }
}

public class StyleBackgroundColor(string cssValue) : IBackgroundColor
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        return;
    }
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("background-color", cssValue);
    }
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