using Aurila.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Contracts.Design;

public interface IArrangement
{
    void BuildClass(ComponentBase component, ClassBuilder builder);

    void BuildStyle(ComponentBase component, StyleBuilder builder);
}