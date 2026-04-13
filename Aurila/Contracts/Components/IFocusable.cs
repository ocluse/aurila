using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Contracts.Components;

public interface IFocusable
{
    Task FocusAsync();

    Task BlurAsync();
}
