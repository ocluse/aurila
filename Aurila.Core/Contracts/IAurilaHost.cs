using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Contracts;

public interface IAurilaHost
{
    event Action<object?>? IntentReceived;

    object? GetLaunchIntent();

    void RequestExit();
}
