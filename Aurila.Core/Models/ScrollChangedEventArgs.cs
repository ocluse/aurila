using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Models;
public class ScrollChangedEventArgs : EventArgs
{
    public double ScrollPosition { get; init; }

    public double ScrollSize { get; init; }

    public double ClientSize { get; init; }

    public double Progress { get; init; }
}
