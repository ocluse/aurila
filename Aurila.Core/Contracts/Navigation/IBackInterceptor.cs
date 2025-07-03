using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Contracts.Navigation;

public interface IBackInterceptor
{
    bool OnBackButtonPressed();
    void RegisterBackReceiver(IBackReceiver receiver);
    void UnregisterBackReceiver(IBackReceiver receiver);
}
