using Aurila.Components;
using Aurila.Contracts.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Modifiers;

internal class OffsetModifier(CssLength x, CssLength y) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("transform", $"translate({x}, {y})");
    }
}
