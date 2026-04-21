using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Components.Modals;

public class NavigatedEventArgs(Type currentPageType, string? currentPageRoute) : EventArgs
{
    public Type CurrentPageType { get; init; } = currentPageType;

    public string? CurrentRoute { get; init; } = currentPageRoute;
}
