using Aurila.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Appearance;

public class SimpleStaticAppearance<T>(string? style, string? classNames) : IStaticAppearance<T>
    where T : ControlBase<T>
{
    public string? Class => classNames;

    public string? Style => style;
}
