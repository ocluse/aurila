using Aurila.Components;
using Aurila.Contracts.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Modifiers;

public class FillMaxSizeModifier() : IModifier
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        // No class to add for this modifier
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("width", "100%");
        builder.Add("height", "100%");
        builder.Add("box-sizing", "border-box");
        builder.Add("flex-shrink", "0");
        builder.Add("flex-grow", "0");
        builder.Add("flex-basis", "100%");
    }
}
