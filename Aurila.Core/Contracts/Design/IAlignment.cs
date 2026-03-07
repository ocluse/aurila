using Aurila.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Contracts.Design;

public interface IAlignment
{
    void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder);
    void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder);
}
