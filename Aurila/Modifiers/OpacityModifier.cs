using Aurila.Components;
using Aurila.Contracts.Modifiers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Aurila.Modifiers;

internal class OpacityModifier(double opacity) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("opacity", opacity.ToString(CultureInfo.InvariantCulture));
    }
}
