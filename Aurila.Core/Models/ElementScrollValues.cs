using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Models;
public record ElementScrollValues
{
    public double ScrollTop { get; set; }
    public double ScrollLeft { get; set; }
    public double ScrollHeight { get; set; }
    public double ScrollWidth { get; set; }
    public double ClientHeight { get; set; }
    public double ClientWidth { get; set; }
}