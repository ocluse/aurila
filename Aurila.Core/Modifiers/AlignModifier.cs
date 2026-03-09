using Aurila.Components;
using Aurila.Contracts.Design;
using Aurila.Contracts.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Modifiers;

internal class AlignModifier(IAlignment alignment) : IClassModifier, IStyleModifier
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        alignment.BuildClass(LayoutScope.Self, component, builder);
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        alignment.BuildStyle(LayoutScope.Self, component, builder);
    }
}
