using Aurila.Contracts.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Services;
internal class BackInterceptor : IBackInterceptor
{
    private readonly List<IBackReceiver> _receivers = [];

    public bool OnBackButtonPressed()
    {
        if (_receivers.Count == 0)
        {
            return false;
        }
        var receiver = _receivers[^1];
        return receiver.HandleBackPressed();
    }

    public void RegisterBackReceiver(IBackReceiver receiver)
    {
        _receivers.Add(receiver);
    }

    public void UnregisterBackReceiver(IBackReceiver receiver)
    {
        _receivers.Remove(receiver);
    }
}
