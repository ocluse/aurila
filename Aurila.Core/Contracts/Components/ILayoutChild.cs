using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Contracts.Components;

public interface ILayoutChild
{
    ILayoutParent? Parent { get; }
}
