using Aurila.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Contracts.Modifiers;

public interface IModifier
{
    void BuildClass(ComponentBase component, ClassBuilder builder);

    void BuildStyle(ComponentBase component, StyleBuilder builder);
}