using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Contracts.Components;
public interface IScrollController
{
    event EventHandler<ScrollChangedEventArgs> ScrollChanged;

    Task ScrollToPositionAsync(int positionPx);

    Task ScrollToPositionAsync(double progress);

    Task ScrollToEndAsync();

    Task ScrollToStartAsync();
}
