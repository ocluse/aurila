using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Contracts.Components;
public interface IModal
{
    Task ShowAsync();
    Task HideAsync();
}
