using Aurila.Components;
using Aurila.Contracts.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Modifiers;

internal class OffsetModifier(CssLength x, CssLength y) : IModifier
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        //no op;
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("transform", $"translate({x}, {y})");
    }
}
